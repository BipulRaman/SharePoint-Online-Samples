var strListTitle = "MY LIST NAME";
var strListName = "MY_x0020_LIST_x0020_NAME";
var ID = "4"; // Item ID
var data = {
  __metadata: { 'type': 'SP.Data.'+ strListName +'ListItem' },
	'Title': 'ABC',
  'Another_x0020_Field' : 'Some Value' 
};	

$.ajax({
  type: "POST",
  async: true,
  headers: {
    "accept": "application/json;odata=verbose",
    "content-type": "application/json;odata=verbose",
    "X-RequestDigest": $("#__REQUESTDIGEST").val(),
    "IF-MATCH": "*",
    "X-HTTP-Method":"MERGE"    
  },
  url: _spPageContextInfo.webAbsoluteUrl + '/_api/web/Lists/GetByTitle(\'' + strListTitle + '\')/items('+ ID +')',
  data: JSON.stringify(data),
  success: function (data) {
    console.log('Item Added!');
  },
  failure: function (error) { alert('Error in AJAX call to SP REST API: ' + error.message); }
});
