namespace SharedLibrary.Structures
{
    /// <summary>
    /// PasswordChangeStructure is used when changing password and it is expected by the controller for changing password.
    /// </summary>
    public class PasswordChangeStructure
        {
            /// <summary>
            /// OldPassword property.
            /// </summary>
            /// <value>OldPassword represents current password value.</value>
            public string OldPassword { get; set; }
            /// <summary>
            /// New Password property.
            /// </summary>
            /// <value>New Password represents the value the password should be changed to.</value>
            public string NewPassword { get; set; }
            /// <summary>
            /// NewPasswordCopy property.
            /// </summary>
            /// <value>NewPasswordCopy must have the same value as NewPassword and is used for validation./value>
            public string NewPasswordCopy { get; set; }
        }
}