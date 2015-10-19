var hackIntervalId = 0;

LazyLoad.js("##JsInitialize##", function () {
    //we need to set the legacy UmbClientMgr path

    // IE for some weird reason first runs this and then loads LegacyUmbClientMgr.js...
    if (typeof UmbClientMgr === "undefined") {
        hackIntervalId = setInterval(load, 5000);
    } else {
        load();
    }
});

function load() {
    if (hackIntervalId != 0) {
        clearInterval(hackIntervalId);
    }

    UmbClientMgr.setUmbracoPath('"##UmbracoPath##"');

    jQuery(document).ready(function () {

        angular.bootstrap(document, ['umbraco']);

    });
}
