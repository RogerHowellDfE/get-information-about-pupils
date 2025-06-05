namespace DfE.GIAP.Common.Constants.Messages.Search
{
    public static class SearchErrorMessages
    {
        public const string DobInvalid = "The date of birth field is invalid";

        public const string FilterEmpty = "You have not entered any text";
        public const string CheckboxNotSelected = "You have not made a selection";

        public const string SelectOneOrMoreDataTypes = "Select one or more data types";
        public const string SelectFileType = "Select the file type to download";

        public const string NoUPNsFound = "Some of the UPN(s) have not been found in our database. Please see results table for details";
        public const string NoULNsFound = "Some of the ULN(s) have not been found in our database. Please see results table for details";

        public const string EnterUPNs = "Enter one or more UPNs";
        public const string EnterULNs = "Enter one or more ULNs";

        public const string TooManyUPNs = "Too many UPNs";
        public const string TooManyULNs = "Too many ULNs";

        public const string UPNLength = "The UPN must be 13 characters";
        public const string UPNFormat = "The UPN must be in the correct format";
        public const string UPNMustBeUnique = "The ULN must be unique";

        public const string ULNLength = "The ULN must be 10 digits";
        public const string ULNFormat = "The ULN must be in the correct format";
        public const string ULNMustBeUnique = "The ULN must be unique";

        public const string NoSearchText = "You have not entered a first name and/or surname";
        public const string SearchTextPlaceholder = "Enter a first name and/or surname";

        public const string NoDataFound = "No pupil data found, try again";
    }
}
