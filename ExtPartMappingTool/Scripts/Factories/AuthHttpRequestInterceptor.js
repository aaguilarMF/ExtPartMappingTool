var AuthHttpRequestInterceptor = function ($q, $location, $templateCache) {
    return {
        request: function (config) {

            var baseUrl = $("base").first().attr("href");
            if (!$templateCache.get(config.url)) {
                config.url = baseUrl != undefined ? baseUrl + config.url : '' + config.url;
            }
            return config || $q.when(config);
        }
    }
}

AuthHttpRequestInterceptor.$inject = ['$q', '$location', '$templateCache'];