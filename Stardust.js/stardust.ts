
/// <reference path="../Stardust.Starterkit.Proxy/scripts/ClientApi/HttpClient.ts" />
/// <reference path="../Stardust.Starterkit.Proxy/scripts/typings/jquery/jquery.d.ts" />
namespace Stardust.js {
    var stor: Storage;

    export class ConfigurationManager {
        httpClient: HttpClient;
        constructor(public baseUri: string = HttpClient.locationOrigin, public error?: (xhr: JQueryXHR, ajaxOptions: string, thrown: string) => any, public statusCode?: { [key: string]: any; }) {
            this.httpClient = new HttpClient();
            
        }

        /** 
         * GET js/{configSet}/{environment}/{key}
         * @param {string} configSet 
         * @param {string} environment 
         * @param {string} key 
         * @return {any} 
         */
        getByConfigsetAndEnvironmentAndKey(configSet: string, environment: string, key: string, callback: (data: any) => string) {
            var key = configSet + "/" + environment + "/" + key;
            stor = new Storage();
            var value = stor[key];
            if (value != null && value !== "")
                return value;
            value = this.httpClient.get(encodeURI(this.baseUri + 'js/' + configSet + '/' + environment + '/' + key), callback, this.error, this.statusCode);
            stor.setItem(key,value);
            return value;
        }

        /** 
         * POST js/{configSet}/{environment}/{key}
         * @param {string} configSet 
         * @param {string} environment 
         * @param {string} key 
         * @return {any} 
         */
        getByConfigsetAndEnvironmentAndKeyPost(configSet: string, environment: string, key: string, callback: (data: any) => string) {
            this.httpClient.post(encodeURI(this.baseUri + 'js/' + configSet + '/' + environment + '/' + key), null, callback, this.error, this.statusCode);
        }

        /** 
         * POST js/{configSet}/{environment}/{host}/{key}
         * @param {string} configSet 
         * @param {string} environment 
         * @param {string} host 
         * @param {string} key 
         * @return {any} 
         */
        getHostParameterByConfigsetAndEnvironmentAndHostAndKeyPost(configSet: string, environment: string, host: string, key: string, callback: (data: any) => any) {
            this.httpClient.post(encodeURI(this.baseUri + 'js/' + configSet + '/' + environment + '/' + host + '/' + key), null, callback, this.error, this.statusCode);
        }

        /** 
         * GET js/{configSet}/{environment}/{host}/{key}
         * @param {string} configSet 
         * @param {string} environment 
         * @param {string} host 
         * @param {string} key 
         * @return {any} 
         */
        getHostParameterByConfigsetAndEnvironmentAndHostAndKey(configSet: string, environment: string, host: string, key: string, callback: (data: any) => any) {
            this.httpClient.get(encodeURI(this.baseUri + 'js/' + configSet + '/' + environment + '/' + host + '/' + key), callback, this.error, this.statusCode);
        }
    }

    export class ConfigReader {
        httpClient: HttpClient;
        constructor(public baseUri: string = HttpClient.locationOrigin, public error?: (xhr: JQueryXHR, ajaxOptions: string, thrown: string) => any, public statusCode?: { [key: string]: any; }) {
            this.httpClient = new HttpClient();
        }

        /** 
         * GET api/ConfigReader/{id}?env={env}&updKey={updKey}
         * @param {string} id 
         * @param {string} env 
         * @param {string} updKey 
         * @return {any} 
         */
        get(id: string, env: string, updKey: string, callback: (data: any) => any) {
            this.httpClient.get(encodeURI(this.baseUri + 'api/ConfigReader/' + id + '?env=' + env + '&updKey=' + updKey), callback, this.error, this.statusCode);
        }
    }

    var configManager: ConfigurationManager;

    export class StardustConfiguration  {

        constructor() {

        }

        start() {
            configManager = new ConfigurationManager(document.cookie["cnfl"]);
            
        }

        stop() {
        }
    }
}

//var config: Stardust.js.StardustConfiguration;
//window.onload = () => {
//    var el = document.getElementById('content');
//    config = new Stardust.js.StardustConfiguration();
//    config.start();
//};
//window.onunload=() => {
//    config.stop();
//}
