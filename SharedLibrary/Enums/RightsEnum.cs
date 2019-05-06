namespace SharedLibrary.Enums
{
    /// <summary>
    /// Enum of rights. This rights are assigned to each application user for each dataset.
    /// </summary>
    public enum RightsEnum
    {
        /// <summary>
        /// RightsEnum.None represents no rights for a dataset.
        /// </summary>
        None = 0,
        /// <summary>
        /// RightsEnum.R represents read rights for a dataset.
        /// </summary>
        R = 1,
        /// <summary>
        /// RightsEnum.CR represents create and read rights for a dataset.
        /// </summary>
        CR = 2,
        /// <summary>
        /// RightsEnum.CRU represents create, read and update rights for a dataset.
        /// </summary>
        CRU = 3,
        /// <summary>
        /// RightsEnum.CRUD represents create, read, update and delete rights for a dataset.
        /// </summary>
        CRUD = 4,
    }
}