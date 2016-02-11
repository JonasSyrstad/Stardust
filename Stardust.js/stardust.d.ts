/// <reference path="../Stardust.Starterkit.Proxy/scripts/ClientApi/HttpClient.d.ts" />
/// <reference path="../Stardust.Starterkit.Proxy/scripts/typings/jquery/jquery.d.ts" />
declare namespace Stardust.js {
    class ConfigurationManager {
        baseUri: string;
        error: (xhr: JQueryXHR, ajaxOptions: string, thrown: string) => any;
        statusCode: {
            [key: string]: any;
        };
        httpClient: HttpClient;
        constructor(baseUri?: string, error?: (xhr: JQueryXHR, ajaxOptions: string, thrown: string) => any, statusCode?: {
            [key: string]: any;
        });
        /**
         * GET js/{configSet}/{environment}/{key}
         * @param {string} configSet
         * @param {string} environment
         * @param {string} key
         * @return {any}
         */
        getByConfigsetAndEnvironmentAndKey(configSet: string, environment: string, key: string, callback: (data: any) => string): any;
        /**
         * POST js/{configSet}/{environment}/{key}
         * @param {string} configSet
         * @param {string} environment
         * @param {string} key
         * @return {any}
         */
        getByConfigsetAndEnvironmentAndKeyPost(configSet: string, environment: string, key: string, callback: (data: any) => string): void;
        /**
         * POST js/{configSet}/{environment}/{host}/{key}
         * @param {string} configSet
         * @param {string} environment
         * @param {string} host
         * @param {string} key
         * @return {any}
         */
        getHostParameterByConfigsetAndEnvironmentAndHostAndKeyPost(configSet: string, environment: string, host: string, key: string, callback: (data: any) => any): void;
        /**
         * GET js/{configSet}/{environment}/{host}/{key}
         * @param {string} configSet
         * @param {string} environment
         * @param {string} host
         * @param {string} key
         * @return {any}
         */
        getHostParameterByConfigsetAndEnvironmentAndHostAndKey(configSet: string, environment: string, host: string, key: string, callback: (data: any) => any): void;
    }
    class ConfigReader {
        baseUri: string;
        error: (xhr: JQueryXHR, ajaxOptions: string, thrown: string) => any;
        statusCode: {
            [key: string]: any;
        };
        httpClient: HttpClient;
        constructor(baseUri?: string, error?: (xhr: JQueryXHR, ajaxOptions: string, thrown: string) => any, statusCode?: {
            [key: string]: any;
        });
        /**
         * GET api/ConfigReader/{id}?env={env}&updKey={updKey}
         * @param {string} id
         * @param {string} env
         * @param {string} updKey
         * @return {any}
         */
        get(id: string, env: string, updKey: string, callback: (data: any) => any): void;
    }
    class StardustConfiguration {
        constructor();
        start(): void;
        stop(): void;
    }
}
