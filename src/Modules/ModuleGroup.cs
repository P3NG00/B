namespace B.Modules
{
    public sealed class ModuleGroup
    {
        #region Public Properties

        // Title of this group of modules.
        public readonly string GroupTitle;
        // Types of modules in this group.
        public readonly Type[] ModuleTypes;

        #endregion



        #region Constructors

        // Creates a new Module Group.
        public ModuleGroup(string groupTitle, params Type[] moduleTypes)
        {
            GroupTitle = groupTitle;
            ModuleTypes = moduleTypes;
        }

        #endregion
    }
}
