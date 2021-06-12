var urlParams = new URLSearchParams(window.location.search);
var appWebUrl = urlParams.get('SPAppWebUrl');
var hostWebUrl = urlParams.get('SPHostUrl');

SP.SOD.loadMultiple(['sp.js', 'SP.RequestExecutor.js'], initializePage);

function initializePage() {   
    // This code runs when the DOM is ready and creates a context object which is needed to use the SharePoint object model
    $(document).ready(function () {
        UpdateListItem();
    });    

    function UpdateListItem() {
        // Get content of SPHostweb with respect to SPAppWeb
        var clientContext = new SP.ClientContext(appWebUrl);
        var factory = new SP.ProxyWebRequestExecutorFactory(appWebUrl);
        clientContext.set_webRequestExecutorFactory(factory);
        var appContextSite = new SP.AppContextSite(clientContext, hostWebUrl);
        var oWeb = appContextSite.get_web();

        var oList = oWeb.get_lists().getByTitle("CountryData");

        this.itemId = 17;
        this.oListItem = oList.getItemById(itemId);
        oListItem.set_item('Title', 'Bipul Test');
        oListItem.update();
                
        clientContext.executeQueryAsync(
            OnSuccess.bind(this),
            OnFailure
        );
    }

    // Function to handle the success event.
    function OnSuccess() {
        alert('Item updated: ' + itemId);
    }

    // Function to handle the failure event for UpdateListItem.
    function OnFailure(sender, args) {
        alert("Error occurred at UpdateListItem. See console for details.");
        console.log('Request failed at UpdateListItem :' + args.get_message() + '\n' + args.get_stackTrace());
    }
}
