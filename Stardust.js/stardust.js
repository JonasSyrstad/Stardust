/// <reference path="../Stardust.Starterkit.Proxy/scripts/ClientApi/HttpClient.ts" />
/// <reference path="../Stardust.Starterkit.Proxy/scripts/typings/jquery/jquery.d.ts" />
var Stardust;
(function (Stardust) {
    var js;
    (function (js) {
        var stor;
        var ConfigurationManager = (function () {
            function ConfigurationManager(baseUri, error, statusCode) {
                if (baseUri === void 0) { baseUri = HttpClient.locationOrigin; }
                this.baseUri = baseUri;
                this.error = error;
                this.statusCode = statusCode;
                this.httpClient = new HttpClient();
            }
            /**
             * GET js/{configSet}/{environment}/{key}
             * @param {string} configSet
             * @param {string} environment
             * @param {string} key
             * @return {any}
             */
            ConfigurationManager.prototype.getByConfigsetAndEnvironmentAndKey = function (configSet, environment, key, callback) {
                var key = configSet + "/" + environment + "/" + key;
                stor = new Storage();
                var value = stor[key];
                if (value != null && value !== "")
                    return value;
                value = this.httpClient.get(encodeURI(this.baseUri + 'js/' + configSet + '/' + environment + '/' + key), callback, this.error, this.statusCode);
                stor.setItem(key, value);
                return value;
            };
            /**
             * POST js/{configSet}/{environment}/{key}
             * @param {string} configSet
             * @param {string} environment
             * @param {string} key
             * @return {any}
             */
            ConfigurationManager.prototype.getByConfigsetAndEnvironmentAndKeyPost = function (configSet, environment, key, callback) {
                this.httpClient.post(encodeURI(this.baseUri + 'js/' + configSet + '/' + environment + '/' + key), null, callback, this.error, this.statusCode);
            };
            /**
             * POST js/{configSet}/{environment}/{host}/{key}
             * @param {string} configSet
             * @param {string} environment
             * @param {string} host
             * @param {string} key
             * @return {any}
             */
            ConfigurationManager.prototype.getHostParameterByConfigsetAndEnvironmentAndHostAndKeyPost = function (configSet, environment, host, key, callback) {
                this.httpClient.post(encodeURI(this.baseUri + 'js/' + configSet + '/' + environment + '/' + host + '/' + key), null, callback, this.error, this.statusCode);
            };
            /**
             * GET js/{configSet}/{environment}/{host}/{key}
             * @param {string} configSet
             * @param {string} environment
             * @param {string} host
             * @param {string} key
             * @return {any}
             */
            ConfigurationManager.prototype.getHostParameterByConfigsetAndEnvironmentAndHostAndKey = function (configSet, environment, host, key, callback) {
                this.httpClient.get(encodeURI(this.baseUri + 'js/' + configSet + '/' + environment + '/' + host + '/' + key), callback, this.error, this.statusCode);
            };
            return ConfigurationManager;
        })();
        js.ConfigurationManager = ConfigurationManager;
        var ConfigReader = (function () {
            function ConfigReader(baseUri, error, statusCode) {
                if (baseUri === void 0) { baseUri = HttpClient.locationOrigin; }
                this.baseUri = baseUri;
                this.error = error;
                this.statusCode = statusCode;
                this.httpClient = new HttpClient();
            }
            /**
             * GET api/ConfigReader/{id}?env={env}&updKey={updKey}
             * @param {string} id
             * @param {string} env
             * @param {string} updKey
             * @return {any}
             */
            ConfigReader.prototype.get = function (id, env, updKey, callback) {
                this.httpClient.get(encodeURI(this.baseUri + 'api/ConfigReader/' + id + '?env=' + env + '&updKey=' + updKey), callback, this.error, this.statusCode);
            };
            return ConfigReader;
        })();
        js.ConfigReader = ConfigReader;
        var configManager;
        var StardustConfiguration = (function () {
            function StardustConfiguration() {
            }
            StardustConfiguration.prototype.start = function () {
                configManager = new ConfigurationManager(document.cookie["cnfl"]);
            };
            StardustConfiguration.prototype.stop = function () {
            };
            return StardustConfiguration;
        })();
        js.StardustConfiguration = StardustConfiguration;
    })(js = Stardust.js || (Stardust.js = {}));
})(Stardust || (Stardust = {}));
//var config: Stardust.js.StardustConfiguration;
//window.onload = () => {
//    var el = document.getElementById('content');
//    config = new Stardust.js.StardustConfiguration();
//    config.start();
//};
//window.onunload=() => {
//    config.stop();
//}
//# sourceMappingURL=stardust.js.map