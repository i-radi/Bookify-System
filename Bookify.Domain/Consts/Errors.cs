namespace Bookify.Domain.Consts
{
    public static class Errors
    {
        public const string RequiredField = "Required field";
        public const string MaxLength = "{PropertyName} cannot be more than {MaxLength} characters";
        public const string MaxMinLength = "The {PropertyName} must be at least {MinLength} and at max {MaxLength} characters long.";
        public const string Duplicated = "Another record with the same {0} is already exists!";
        public const string DuplicatedBook = "Book with the same title is already exists with the same author!";
        public const string NotAllowedExtension = "Only .png, .jpg, .jpeg files are allowed!";
        public const string MaxSize = "File cannot be more that 2 MB!";
        public const string NotAllowFutureDates = "Date cannot be in the future!";
        public const string InvalidRange = "{PropertyName} should be between {From} and {To}!";
        public const string ConfirmPasswordNotMatch = "The password and confirmation password do not match.";
        public const string WeakPassword = "Passwords contain an uppercase character, lowercase character, a digit, and a non-alphanumeric character. Passwords must be at least 8 characters long";
        public const string InvalidUsername = "Username can only contain letters or digits.";
        public const string OnlyEnglishLetters = "Only English letters are allowed.";
        public const string OnlyArabicLetters = "Only Arabic letters are allowed.";
        public const string OnlyNumbersAndLetters = "Only Arabic/English letters or digits are allowed.";
        public const string DenySpecialCharacters = "Special characters are not allowed.";
        public const string InvalidMobileNumber = "Invalid mobile number.";
        public const string InvalidNationalId = "Invalid national ID.";
        public const string InvalidSerialNumber = "Invalid serial number.";
        public const string NotAvilableRental = "This book/copy is not available for rental.";
        public const string EmptyImage = "Please select an image.";
        public const string NotFoundSubscriber = "This subscriber is found.";
        public const string BlackListedSubscriber = "This subscriber is blacklisted.";
        public const string InactiveSubscriber = "This subscriber is inactive.";
        public const string MaxCopiesReached = "This subscriber has reached the max number for rentals.";
        public const string CopyIsInRental = "This copy is already rentaled.";
        public const string RentalNotAllowedForBlacklisted = "Rental cannot be extended for blacklisted subscribers.";
        public const string RentalNotAllowedForInactive = "Rental cannot be extended for this subscriber before renwal.";
        public const string ExtendNotAllowed = "Rental cannot be extended.";
        public const string PenaltyShouldBePaid = "Penalty should be paid.";
        public const string InvalidStartDate = "Invalid start date.";
        public const string InvalidEndDate = "Invalid end date.";
    }
}