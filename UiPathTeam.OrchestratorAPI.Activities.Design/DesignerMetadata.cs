using System.Activities.Presentation.Metadata;
using System.ComponentModel;
using System.ComponentModel.Design;
using UiPathTeam.OrchestratorAPI.Activities.Design.Designers;
using UiPathTeam.OrchestratorAPI.Activities.Design.Properties;

namespace UiPathTeam.OrchestratorAPI.Activities.Design
{
    public class DesignerMetadata : IRegisterMetadata
    {
        public void Register()
        {
            var builder = new AttributeTableBuilder();
            builder.ValidateTable();

            var categoryAttribute = new CategoryAttribute($"{Resources.Category}");

            builder.AddCustomAttributes(typeof(OrchestratorScope), categoryAttribute);
            builder.AddCustomAttributes(typeof(OrchestratorScope), new DesignerAttribute(typeof(OrchestratorScopeDesigner)));
            builder.AddCustomAttributes(typeof(OrchestratorScope), new HelpKeywordAttribute(""));


            MetadataStore.AddAttributeTable(builder.CreateTable());
        }
    }
}
