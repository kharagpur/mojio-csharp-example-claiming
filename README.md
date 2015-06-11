# Mojio C# Client Example - Console application for Claiming and Un-claiming Mojios

Purpose of this project is to demonstrage automation of claiming Mojio, un-claming Mojio and new User Creation using Mojio Client SDK


### Instructions:
1. Open Application.cs - Main file
2. Main file is depended on few variable that needs to be set before building the solution.
```cs
// APP INFO
Guid appId = new Guid("00000000-0000-0000-0000-000000000000"); // Application Id
Guid secretKey = new Guid("00000000-0000-0000-0000-000000000000"); // Sandbox
//Guid secretKey = new Guid("00000000-0000-0000-0000-000000000000"); // Live

// LOG-IN CREDENTIAL
// USER ACCOUNT YOU WANT TO CLAIM/UN-CLAIM UNDER
var adminUserOrEmail = "yourEmail@email.com";
var adminPassword = "yourPassword";

```
3. Build the solution.  (Build -> Build Soution)
4. Start the application by clicking on the Start button.
5. Once the admin user is authentication, a successful message will be displayed.
```cs
Log In: Successful
```
6. You will be asked to enter the path of csv file you want to claim or unclaim.  Each row of the csv file is expected to be in two columns, serial number and an IMEI number.  If the row is different than the way I intended to be, please modify the code in the following areas:
* Application.cs
```cs
private static void CreateAndAssignUserToUnclaimedMojio(MojioClient client, string fileName)
```
* ClaimMojio.cs
```cs
public static void ClaimMojios(MojioClient client, string fileName)
```
* UnclaimMojio.cs
```cs
public static void UnclaimMojios(MojioClient client, string fileName)
```
7. For user creation, I have a pre-populated the pattern in the code and a number will be appended at the end of the pattern, for unique username, password and email.  Feel free to modify this pattern to your use case.  This can be modified in:
* Application.cs
```cs
private static void CreateAndAssignUserToUnclaimedMojio(MojioClient client, string fileName)
```