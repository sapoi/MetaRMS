namespace SharedLibrary.Enums
{
    /// <summary>
    /// Enum of message types.
    /// </summary>
    public enum OnDeleteActionEnum
    {
        /// <summary>
        /// OnDeleteActionEnum.None represents undefined on delete action. This value is for every 
        /// attribute of basic type and is forbidden for values of reference type.
        /// </summary>
        None = 0,
        /// <summary>
        /// OnDeleteActionEnum.SetEmpty in attribute of type reference means, that when model referenced 
        /// in the attribute is deleted, its value in the attribute will be replaced with empty value. 
        /// OnDeleteActionEnum.SetEmpty can not be used for non reference types.
        /// </summary>
        SetEmpty = 1,
        /// <summary>
        /// OnDeleteActionEnum.Cascade in attribute of type reference means, that when model referenced 
        /// in that attribute is deleted, the model having it as attribute value will be deleted as well 
        /// if possible. OnDeleteActionEnum.Cascade can not be used for non reference types. Also it can 
        /// not be used in attributes of type system users dataset reference so that the last user of the 
        /// application cannot be accidentally deleted.
        /// </summary>
        Cascade = 2,
        /// <summary>
        /// OnDeleteActionEnum.Protect in attribute of type reference means, that if delete action on model 
        /// referenced in that attribute is performed, it is stopped due to this protection and none of
        /// the models is deleted. OnDeleteActionEnum.Cascade can not be used for non reference types.
        /// </summary>
        Protect = 3
    }
}