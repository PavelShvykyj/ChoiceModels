using System.Net.Http.Headers;

namespace ChoiceLocalService.Services.Delegates
{
    public class HttpApiDelegate : IMessageDelegate
    {
        private readonly ILogger<HttpApiDelegate> _logger;
        private readonly HttpClient _http;
        private readonly SemaphoreSlim _sessionLock = new(1, 1);
        private string? _sessionId;
        private readonly string _pingPath;
        private readonly string _eventPath;

        private readonly AuthenticationHeaderValue _auth;
        public bool IsEnabled => _sessionId is not null;

        // События

        public event Func<bool, Task>? SessionStateChanged;



        public HttpApiDelegate(
            ILogger<HttpApiDelegate> logger,
            HttpClient httpClient,
            IConfiguration config)
        {
            _logger = logger;
            _http = httpClient;
            //var baseUrl = config["Api:BaseUrl"] ?? throw new InvalidOperationException("Api:BaseUrl not configured");

            //_http.BaseAddress = new Uri(baseUrl);

            _pingPath = config["Api:PingPath"] ?? "/ping";
            _eventPath = config["Api:EventPath"] ?? "/event";

            var user = config["Api:User"];
            var pass = config["Api:Password"];

            _auth = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{user}:{pass}")));

            _logger.LogDebug($"url: {_pingPath}");

        }

        // === API управления ===

        public async Task<bool> EnableAsync()
        {
            if (await EnsureSessionAsync())
            {
                
                _logger.LogInformation("🟢 HttpApiDelegate ENABLED manually.");
                return true;
            }
            else {
                _logger.LogWarning("⛔ Fail to enable HttpApiDelegate");
                return false;
            }
        }

        public async Task<bool> DisableAsync()
        {
            await _sessionLock.WaitAsync();
            try 
            { 
                await LogoutAsync();
                _logger.LogWarning("⛔ HttpApiDelegate DISABLED manually.");
                _sessionId = null;
                 await SessionStateChanged?.Invoke(false);
                return true;
            }
            finally { _sessionLock.Release(); }
        }

        // === Основная логика ===

        public async Task<bool> HandleAsync(string messageBody)
        {


            try
            {
                _logger.LogInformation("📨 Sending order to API (length={len})", messageBody.Length);

                if (!await EnsureSessionAsync())
                {
                    _logger.LogError("Failed to establish session.");
                    return false;
                }

                using var req = new HttpRequestMessage(HttpMethod.Post, _eventPath);
                req.Headers.Authorization = _auth;
                req.Headers.TryAddWithoutValidation("Cookie", $"IBsession={_sessionId}");
                req.Content = new StringContent(messageBody, System.Text.Encoding.UTF8, "application/json");

                var resp = await _http.SendAsync(req);
                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogError("API returned error: {code}", resp.StatusCode);
                    
                    return false;
                }

                _logger.LogInformation("✅ Order successfully sent to API");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Exception during order processing.");
                await SessionStateChanged?.Invoke(false);
                return false;
            }
        }

        private async Task LogoutAsync() {
            try {
                if (_sessionId is null)
                {
                    return;
                }
                using var outReq = new HttpRequestMessage(HttpMethod.Get, _pingPath);
                outReq.Headers.Authorization = _auth;
                outReq.Headers.TryAddWithoutValidation("IBSession", "stop");
                outReq.Headers.TryAddWithoutValidation("Cookie", $"ibsession={_sessionId}");
                await _http.SendAsync(outReq);

            }
            catch {
                return;
            }
        }

        private async Task<string?> LoginAsync() {
            try
            {
                await LogoutAsync();
                using var startReq = new HttpRequestMessage(HttpMethod.Get, _pingPath);
                startReq.Headers.Authorization = _auth;
                startReq.Headers.TryAddWithoutValidation("IBSession", "start");

                var resp = await _http.SendAsync(startReq);
                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogError("Cannot start session: {code}", resp.StatusCode);
                    return null;
                }

                if (!TryExtractIbsession(resp, out var sid))
                {
                    _logger.LogError("Failed to parse ibsession cookie.");

                    return null;
                }
                _logger.LogInformation("✅ New IBSession started: {sid}", sid);
                return sid;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error loggin on EnsureSessionAsync.");
            return null;
        }
}

        private async Task<bool> PingAsync() {
            try
            {
                using var pingReq = new HttpRequestMessage(HttpMethod.Get, _pingPath);
                pingReq.Headers.Authorization = _auth;
                pingReq.Headers.TryAddWithoutValidation("Cookie", $"ibsession={_sessionId}");
                var resp = await _http.SendAsync(pingReq);
                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogError("Cannot start session: {code}", resp.StatusCode);
                    return false;
                }
                return true;

            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error loggin on EnsureSessionAsync {ex.Message}");
                return false;
            }
        }

        private async Task<bool> EnsureSessionAsync()
        {
            await _sessionLock.WaitAsync();
            var result = false;
            bool sessionState = _sessionId is not null;
            try
            {
                if (_sessionId is null)
                {
                    _sessionId = await LoginAsync();
                    result = _sessionId is not null;
                }
                else
                {
                    result = await PingAsync();
                    if (!result)
                    {
                        _sessionId = await LoginAsync();
                        result = _sessionId is not null;
                    }                                    
                }

                if (sessionState != result || _sessionId is null)
                {    
                    await SessionStateChanged?.Invoke( result);
                }
                
                return result;
            }
            finally
            {
                _sessionLock.Release();
            }
        }

        private static bool TryExtractIbsession(HttpResponseMessage resp, out string? sid)
        {
            sid = null;
            if (resp.Headers.TryGetValues("Set-Cookie", out var cookies))
            {
                var match = cookies.FirstOrDefault(v => v.Contains("ibsession"));
                if (match is not null)
                {
                    var parts = match.Split(';')[0].Split('=');
                    if (parts.Length == 2)
                    {
                        sid = parts[1];
                        return true;
                    }
                }
            }
            return false;
        }

    }
}
