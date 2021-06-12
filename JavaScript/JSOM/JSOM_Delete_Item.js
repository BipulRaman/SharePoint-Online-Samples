function DeleteListItem() {
    this.itemId = 11;
    var clientContext = new SP.ClientContext.get_current();    
    var oList = clientContext.get_web().get_lists().getByTitle("CountryData");

    this.oListItem = oList.getItemById(itemId);
    oListItem.deleteObject();

    clientContext.executeQueryAsync(
        OnSuccess.bind(this),
        OnFailure
    );
}

// Function to handle the success event.
function OnSuccess() {
    alert('Item deleted: ' + itemId);
}

// Function to handle the failure event for DeleteListItem.
function OnFailure(sender, args) {
    alert("Error occurred at DeleteListItem. See console for details.");
    console.log('Request failed at DeleteListItems :' + args.get_message() + '\n' + args.get_stackTrace());
}

ExecuteOrDelayUntilScriptLoaded(DeleteListItem, "sp.js");