## Using modern authentication with CSOM for .NET Standard

Using user/password based authentication which was a so called cookie based authentication, implemented via the `SharePointOnlineCredentials` class, is a common approach for developers using CSOM for .NET Framework. In CSOM for .NET Standard this isn't possible anymore, it's up to the developer using CSOM for .NET Standard to obtain an OAuth access token and use that when making calls to SharePoint Online. The recommended approach for getting access tokens for SharePoint Online is by setting up an Azure AD application. For CSOM for .NET Standard the only thing that matters are that you obtain a valid access token, this can be using resource owner password credential flow, using device login, using certificate based auth,...  

In this sample, we'll use an OAuth resource owner password credential flow resulting in an OAuth access token that then is used by CSOM for authenticating requests against SharePoint Online as that mimics the behavior of the `SharePointOnlineCredentials` class.

### Configuring an application in Azure AD

Below steps will help you create and configure an application in Azure Active Directory:

- Go to Azure AD Portal via https://aad.portal.azure.com
- Select **Azure Active Directory** and on  **App registrations** in the left navigation
- Select **New registration**
- Enter a name for your application and select **Register**
- Go to **API permissions** to grant permissions to your application, select **Add a permission**, choose **SharePoint**, **Delegated permissions** and select for example **AllSites.Manage**
- Select **Grant admin consent** to consent the application's requested permissions
- Select **Authentication** in the left navigation
- Change **Allow public client flows** from No to **Yes**
- Select **Overview** and copy the application ID to the clipboard (you'll need it later on)