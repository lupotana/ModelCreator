using System;

namespace ModelCreator
{
    public enum GeneratorLayer
    {
        OneLayer,
        ThreeLayer
    }

    public enum GeneratorOutput
    {
        DataTable,
        Collection
    }

    public enum Modality
    {
        Professional,
        Easy,
        ThreeLayer
    }

    public enum Provider
    {
        BaseProvider,
        SqlServer,
        SqlBase,
        SqlBase2,
        MySql,
        Access,
        Access2,
        PostGres,
        Oracle,
        AutoTask
    }

    public enum WebControl
    {
        DropDownList,
        TextBox,
        Calendar,
        CheckBox,
    }

    public enum Framework
    {
        net20,
        net35,
        net40,
        net45
    }
   


}
