function UpdateListItem() {
    var clientContext = new SP.ClientContext.get_current();    
    var oList = clientContext.get_web().get_lists().getByTitle("CountryData");

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

ExecuteOrDelayUntilScriptLoaded(UpdateListItem, "sp.js");