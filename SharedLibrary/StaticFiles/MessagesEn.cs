using System.Collections.Generic;

namespace SharedLibrary.StaticFiles
{
    public static class MessagesEn
    {
        //TODO move this into a file
        public static Dictionary<int, string> Messages = new Dictionary<int, string>()
        {
            // 0xxx APPLICATION INITIALIZATION
            {0001, "JSON file with application descriptor is required."},
            {0002, "{0}"},
            {0003, "Login application name {0} already exists, please choose another. (You can keep Application name the same, the conflict is only with Login application name.)"},
            {0004, "Each dataset name must be unique, but name {0} used {1} times."},
            {0005, "Invalid name of dataset {0}. Names {1} can not be used."},
            {0006, "Invalid name of dataset {0}. Names {number} can not be used."},
            {0007, "Each dataset must contain at least one required attribute, but dataset {0} contains none."},
            {0008, "Each attribute in dataset must have unique name, but in dataset {0} attribute name {1} is used {2} times."},
            {0009, "Password attribute {0} must have its type set to 'password'."},
            {0010, "Invalid reference in dataset {0} attribute {1} is of type {2}, but there is no dataset with such name."},
            {0011, "Attribute {0} in dataset {1} is of type reference, but does not have a on delete action."},
            {0012, "Attribute {0} in dataset {1} is of type system user dataset. Attributes of such type cannot have a on delete action set to cascade."},
            {0013, "Attribute {0} in dataset {1} is of a basic type. Attributes of such type cannot have a on delete action set."},
            {0014, "Password attribute {0} must be required."},
            {0015, "Attribute {0} in dataset {1} cannot be of type 'password'."},
            {0016, "Attribute {0} in dataset {1} cannot have value Safer set."},
            {0017, "Attribute {0} in dataset {1} cannot have Min value greater than Max value."},
            {0018, "Attribute {0} in dataset {1} cannot have Min value less than or equal to 0."},
            {0019, "Attribute {0} in dataset {1} cannot have Max value less than or equal to 0."},
            {0020, "Attribute {0} in dataset {1} cannot be of type 'username'."},
            {0021, "There must be exactly one attribute of type 'username' in dataset {0}, but {1} found."},
            {0022, "Username attribute {0} in dataset {1} must be required."},
            {0023, "Username attribute {0} in dataset {1} must be unique."},
            {0024, "References cannot have Min property set and Required property set to flase, but attribute {0} in dataset {1} has so."},
            {0025, "References with Required property set to true on Min property greater than 0 cannot have OnDeleteAction set to SetEmpty, but attribute {0} in dataset {1} has so."},
            {0026, "Email address {0} is not valid, please choose another."},
            {0027, "Application {0} was created successfully and login credentials were sent to email {1}."},

            // 1xxx GENERAL APPLICATION
            {1001, "Application name is required."},
            {1002, "Username is required."},
            {1003, "Password is required."},
            {1004, "Invalid application name."},
            {1005, "Could not log in: combination of application name {0}, username {1} and password does not exist."},
            {1006, "You are not authorized to read this data."},
            {1007, "Server error - data from server could not be parsed."},

            // 2xxx DATA DATASETS
            {2001, "Dataset with id {0} not found."},
            {2002, "New data in dataset {0} created successfully."},
            {2003, "Combination of application name {0}, dataset {1} and id {2} does not exist."},
            {2004, "Data from dataset {0} deleted successfully."},
            {2005, "Data in dataset {0} edited successfully."},
            {2006, "No element data with id {0} found in dataset {1}."},
            {2007, "You are not allowed to read data of dataset {0}."},
            {2008, "You are not allowed to delete data of dataset {0}."},
            {2009, "You are not allowed to create data of dataset {0}."},
            {2010, "You are not allowed to edit data of dataset {0}."},
            {2011, "You are not allowed to read any application datasets."},
            {2012, "Data can not be deleted, because there is reference to this or related data, that has to be removed first."},

            // 3xxx USERS DATASET
            {3001, "Username can not be an empty string."},
            {3002, "User named {0} already exists, please choose another username."},
            {3003, "New user {0} created successfully with password set to {0}."},
            {3004, "Combination of application name {0} and user id {1} does not exist."},
            {3005, "User {0} deleted successfully."},
            {3006, "No user with id {0} found."},
            {3007, "User {0} edited successfully."},
            {3008, "User {0} password reset successfully to {0}."},
            {3009, "You are not allowed to read users."},
            {3010, "You are not allowed to delete users."},
            {3011, "You are not allowed to create users."},
            {3012, "You are not allowed to edit users."},
            {3013, "Can not delete last user in application. There has to be always at least one."},
            {3014, "User can not be deleted, because there is reference to this or related data, that has to be removed first."},

            // 4xxx RIGHTS DATASET
            {4001, "Rights named {0} already exists, please choose another name."},
            {4002, "New rights {0} created successfully."},
            {4003, "Combination of application name {0} and rights id {1} does not exist."},
            {4004, "Can't delete - rights {0} are used by one or more users: {1}."},
            {4005, "Rights {0} deleted successfully."},
            {4006, "No rights with id {0} found."},
            {4007, "Rights {0} edited successfully."},
            {4008, "You are not allowed to read rights."},
            {4009, "You are not allowed to delete rights."},
            {4010, "You are not allowed to create rights."},
            {4011, "You are not allowed to edit rights."},

            // 5xxx SETTINGS
            {5001, "Each of old, new and the copy of new passwords are required to be non empty."},
            {5002, "New and the copy of new passwords must be equal."},
            {5003, "Old password is incorrect."},
            {5004, "New password does not fulfill the security requirements - it must be at least 8 characters long and contain at least 1 upper and 1 lower-case letter and one number or special character."},
            {5005, "Password changed successfully."},

            // 6xxx VALIDATIONS
            {6001, "Attribute {0} does not exist in dataset {1}."},
            {6002, "Attribute {0} value is required."},
            {6003, "Attribute {0} is not a reference type - only 1 value can be submitted, but {1} were received."},
            {6004, "Attribute {0} value not in a correct format - {1} expected, but {2} received."},
            {6005, "Attribute {0} value can not be less than {1}, but {2} received."},
            {6006, "Attribute {0} value can not be greater than {1}, but {2} received."},
            {6007, "Attribute {0} value can not be shorter than {1}, but value {2} has only {3} characters."},
            {6008, "Attribute {0} value can not be longer than {1}, but value {2} has {3} characters."},
            {6009, "Attribute {0} value can not contain less than {1} reference(s), but {2} received."},
            {6010, "Attribute {0} value can not contain more than {1} reference(s), but {2} received."},
            {6011, "Rights value for dataset {0} is missing."},
            {6012, "Rights value for dataset {0} is in a incorrect format, number between 0 and 4 expected, but {1} received."},
            {6013, "If dataset {0} has read rights, dataset {1} also must have read rights."},
            {6014, "Recieved application id {0} does not correspond to application id {1} of the authorized user."},
            {6015, "Invalid type {0} for attribute {1}. {0} is not a reference type."},
            {6016, "References must be integer numbers, but for attribute {0} value {1} was received."},
            {6017, "Invalid reference {0} for attribute {1}, no such id in the database."}
        };
    }
}