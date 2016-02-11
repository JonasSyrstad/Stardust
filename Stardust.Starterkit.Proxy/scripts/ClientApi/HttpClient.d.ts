/// <reference path="../typings/jquery/jquery.d.ts" />
declare class HttpClient {
    /**
      location.origin may not be working in some releases of IE. And locationOrigin is an alternative implementation
    **/
    static locationOrigin: string;
    /**
    **/
    get(url: string, callback: (data: any) => any, errorCalback: (xhr: JQueryXHR, ajaxOptions: string, thrown: string) => any, statusCodeCallback: Object): void;
    post(url: string, dataToSave: any, callback: (data: any) => any, errorCalback: (xhr: JQueryXHR, ajaxOptions: string, thrown: string) => any, statusCodeCallback: {
        [key: string]: any;
    }): void;
    put(url: string, dataToSave: any, callback: (data: any) => any, errorCalback: (xhr: JQueryXHR, ajaxOptions: string, thrown: string) => any, statusCodeCallback: {
        [key: string]: any;
    }): void;
    delete(url: string, callback: (data: any) => any, errorCalback: (xhr: JQueryXHR, ajaxOptions: string, thrown: string) => any, statusCodeCallback: {
        [key: string]: any;
    }): void;
    private executeAjax(url, dataToSave, httpVerb, callback, errorCallback, statusCodeCallback);
}
