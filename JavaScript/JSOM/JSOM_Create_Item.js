function CreateListItem() {
    var clientContext = new SP.ClientContext.get_current();
    var oList = clientContext.get_web().get_lists().getByTitle("CountryData");

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

ExecuteOrDelayUntilScriptLoaded(CreateListItem, "sp.js");