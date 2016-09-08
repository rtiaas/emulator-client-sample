# emulator-client-sample
Emulator client sample

An example client showing how to obtain a token from the RTIaaS authorization service and connect to the RTIaaS Activity Framework emulator.

    mkdir c:\rtiaas
    cd c:\rtiaas
    git clone https://github.com/rtiaas/emulator-client-sample

Edit the `src\appsettings.json` file and add your clientId and clientSecret values to the configuration. 

    cd src
    dotnet restore
    dotnet run

You will see the following output:

    Calling authorization service at https://tst-shell-rtiaas-01-wsvc-auth.azurewebsites.net/connect/token   
    Authorization token received, connecting to activity service                                             
    Connected..waiting for messages..                                                                        
    (press ctrl+c to quit)


