OpenStack SDK for .NET
======================

The OpenStack SDK for .NET is an SDK, written for the Microsoft .NET platform, providing developers with what they need to write software against `OpenStack <http://openstack.org/>`_, the open source cloud platform.

Quick Start Example
-------------------
The following code will connect to Openstack, and print out all of the containers in the default storage account::

    using System;
    using System.Threading;
    using Openstack;
    using Openstack.Identity;
    using Openstack.Storage;

    var authUri = new Uri("https://region.identity.host.com:12345/v2.0");
    var userName = "user name";
    var password = "password";
    var tenantId = "XXXXXXXXXXXXXX-Project";

    var credential = new OpenStackCredential(authUri, userName, password, tenantId);
    var client = OpenStackClientFactory.CreateClient(credential);

    await client.Connect();

    var storageServiceClient = client.CreateServiceClient<IStorageServiceClient>();
    var storageAccount = await storageServiceClient.GetStorageAccount();
    foreach(var container in storageAccount.Containers)
    {
        Console.WriteLine(container.Name);
    }

For more examples see the *OpenStack/Examples* directory.

Development
-----------
The `homepage for the development effort <https://wiki.openstack.org/wiki/OpenStack-SDK-DotNet>`_ is on the OpenStack Wiki. The .NET SDK is developed through the same processes as the OpenStack services. `Features requests <https://blueprints.launchpad.net/openstack-sdk-dotnet>`_ and `bugs <https://bugss.launchpad.net/openstack-sdk-dotnet>`_ are filed through launchpad.