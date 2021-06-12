function RetrieveListItems() {
    var clientContext = new SP.ClientContext.get_current();
    var oList = clientContext.get_web().get_lists().getByTitle("CountryData");
    
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

ExecuteOrDelayUntilScriptLoaded(RetrieveListItems, "sp.js");