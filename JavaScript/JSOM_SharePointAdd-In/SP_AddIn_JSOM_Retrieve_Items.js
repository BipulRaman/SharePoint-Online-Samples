var urlParams = new URLSearchParams(window.location.search);
var appWebUrl = urlParams.get('SPAppWebUrl');
var hostWebUrl = urlParams.get('SPHostUrl');

SP.SOD.loadMultiple(['sp.js', 'SP.RequestExecutor.js'], initializePage);

function initializePage() {   
    // This code runs when the DOM is ready and creates a context object which is needed to use the SharePoint object model
    $(document).ready(function () {
        RetrieveListItems();
    });    

    function RetrieveListItems() {
        // Get content of SPHostweb with respect to SPAppWeb
        var clientContext = new SP.ClientContext(appWebUrl);
        var factory = new SP.ProxyWebRequestExecutorFactory(appWebUrl);
        clientContext.set_webRequestExecutorFactory(factory);
        var appContextSite = new SP.AppContextSite(clientContext, hostWebUrl);

        var oWeb = appContextSite.get_web();
        var oList = oWeb.get_lists().getByTitle("CountryData");

        var camlString = "<View Scope='RecursiveAll'><Query><OrderBy><FieldRef Name='ID' Ascending='True' /></OrderBy></Query></View>";
        var camlQuery = new SP.CamlQuery();
        camlQuery.set_viewXml(camlString);

        this.listItemCollection = oList.getItems(camlQuery);
        clientContext.load(listItemCollection, "Include(ID, Title)");

        clientContext.executeQueryAsync(
            OnSuccess.bind(this),
            OnFailure
        );
    }

    // Function to handle the success event for RetrieveListItems.
    function OnSuccess() {
        var enumerator = listItemCollection.getEnumerator();
        // iterating data from listItemCollection
        while (enumerator.moveNext()) {
            var results = enumerator.get_current();
            // data can be utilized here.. 
            console.log(results.get_item("ID") + ' -- ' + results.get_item("Title"));
        }
    }

    // Function to handle the failure event for RetrieveListItems.
    function OnFailure(sender, args) {
        alert("Error occurred at RetrieveListItems. See console for details.");
        console.log('Request failed at RetrieveListItems :' + args.get_message() + '\n' + args.get_stackTrace());
    }
}