# Kauppa-Appi
Kauppa-Appi is an Android mobile application designed for grocery shopping, where it is possible to list shoppers and grocery items.

The idea of the application is that the user carries it with them while shopping and marks the listed items to be bought as they progress through the store. Similarly, after placing each marked item in the basket, the user marks it as purchased. The application is like a "shopping companion" that goes along with the user.

### Shoppers page

App includes different pages and the use of the application begins with the user creating a shopper.
Once a shopper has been created, it is stored in the application's memory and does not need to be added again on the next use. The user can simply select the previously created shopper.
The shopper is selected from a separate list, after which the user can proceed within the application.

**Adding a Shopper**

To add a shopper, click on the "add" button and enter the required information, such as the name, in the pop-up fields that appear.
The shopper's ID, creation date, and activity status are automatically saved. By default, the shopper is active in the application.
When the shopper is successfully added, a notification will appear for the user, the shoppers page will be updated, and the new shopper will be displayed in the list.

**Editing a Shopper**

To edit a shopper, first select the shopper and then click on the "edit" button.
Editing is done by entering the necessary information in the pop-up field and clicking ok.
When editing is successful, a notification will be displayed for the user and the list will be updated.
If no shopper is selected, editing cannot be done and the application will notify the user to select a shopper first.

**Deleting Shoppers**

A shopper who has added items to the shopping list, marked items as to be purchased or purchased, cannot be deleted, and an error message will be displayed to the user.
If a grocery shopper is new and has just been added, they can be removed by selecting the shopper and then clicking the "remove" button. When the removal is successful, the user will receive a notification and the list will be updated. If the shopper to be removed is not selected, the removal will not be possible and a notification will prompt the user to select the shopper to be removed.

### Grocery Shopping Page 
The grocery shopping page displays a list of added items. On the first use of the page, the grocery shopping list is empty and items must be added. This page serves the main purpose of the application; adding items to the grocery list, marking them for purchase, and finally as purchased.

**Adding Grocery Items**

Grocery items are added to the list by clicking "Add Items to Grocery List." After clicking, a pop-up form appears where the user inputs the item details: the name and description of the item. The item ID, creation date, last modified date, as well as the active and completion status are saved automatically. The item's activity status is set to active by default, and the completion status is false because the item has not yet been marked as purchased. When an item is successfully added, the user is notified, and the list is updated.

**Editing Grocery Items**

Grocery items can be edited by first selecting the item and then clicking the "edit" button. Editing is done by inputting the necessary information into the pop-up form and then clicking "OK." When editing is successful, the user is notified, and the list is updated to show the edited grocery item. If a grocery item is not selected, editing will not be possible, and the application will prompt the user to select the item to be edited.

**Removing Grocery Items**

Grocery items can be removed by selecting the item and then clicking the "remove" button. When the removal is successful, the user is notified. If a grocery item is not selected, removal will not be possible, and the application will prompt the user to select the item to be removed.

**Marking grocery shopping items as to be purchased**

When a grocery item is marked as to be purchased, it becomes an active purchase. This is done by selecting the item and then clicking the "to be purchased" button.
After clicking the "to be purchased" button, the user receives a notification and the application vibrates.
If a grocery item has not been selected, it cannot be marked as to be purchased, and the application notifies the user to select the item first.

**Marking grocery shopping items as purchased**

To mark a grocery item as purchased, select the item that has previously been marked as to be purchased and click the "mark as purchased" button.
After the grocery item has been marked as purchased, a pop-up window appears where the user can add a comment about the purchase.
Once the item has been successfully marked as purchased, it disappears from the list and the user receives a notification and the application vibrates.

## Application icon and theme

Kauppa-Appi has its own application icon and theme.
The application theme is determined by the theme the user has set on their mobile device, whether it is dark or light mode.

**Application icon**

The application icon is a shopping bag that is displayed on the mobile device's app menu before the application is launched

**Dark mode**

In dark mode, the entire application has a black theme and the colors of texts, buttons, and tables are adjusted to fit the dark mode.

**Light mode**

In light mode, the application has a light appearance with a few bluish details.


~ _The mobile application was implemented using Visual Studio and Xamarin, with C# as the programming language. Behind it runs a Restful API application implemented with ASP .NET Core, which is responsible for carrying out user actions. The Backend has its own repo named "BackEndKauppaAppi". Application and the github version control (pushes and issues) are made in finnish._

