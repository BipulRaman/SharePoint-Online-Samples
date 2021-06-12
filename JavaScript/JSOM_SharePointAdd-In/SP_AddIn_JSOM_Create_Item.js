var urlParams = new URLSearchParams(window.location.search);
var appWebUrl = urlParams.get('SPAppWebUrl');
var hostWebUrl = urlParams.get('SPHostUrl');

SP.SOD.loadMultiple(['sp.js', 'SP.RequestExecutor.js'], initializePage);

function initializePage() {   
    // This code runs when the DOM is ready and creates a context object which is needed to use the SharePoint object model
    $(document).ready(function () {
        CreateListItem();
    });    

    function CreateListItem() {
        // Get content of SPHostweb with respect to SPAppWeb
        var clientContext = new SP.ClientContext(appWebUrl);
        var factory = new SP.ProxyWebRequestExecutorFactory(appWebUrl);
        clientContext.set_webRequestExecutorFactory(factory);
        var appContextSite = new SP.AppContextSite(clientContext, hostWebUrl);
        
        var oWeb = appContextSite.get_web();
        var oList = oWeb.get_lists().getByTitle("CountryData");

        var itemCreateInfo = new SP.ListItemCreationInformation();
        this.oListItem = oList.addItem(itemCreateInfo);

        oListItem.set_item('Title', 'India');
        oListItem.set_item('Capital', 'New Delhi');

        oListItem.update();
        clientContext.load(oListItem);

        clientContext.executeQueryAsync(
            OnSuccess.bind(this),
            OnFailure
        );
    }

    // Function to handle the success event.
    function OnSuccess() {
        alert('Item created: ' + oListItem.get_id());
    }

    // Function to handle the failure event for CreateListItem.
    function OnFailure(sender, args) {
        alert("Error occurred at CreateListItem. See console for details.");
        console.log('Request failed at CreateListItem :' + args.get_message() + '\n' + args.get_stackTrace());
    }
}