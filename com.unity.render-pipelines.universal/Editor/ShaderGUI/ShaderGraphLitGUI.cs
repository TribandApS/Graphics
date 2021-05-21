using System;
using UnityEditor.Rendering.Universal;
using UnityEditor.Rendering.Universal.ShaderGUI;
using UnityEngine;
using static Unity.Rendering.Universal.ShaderUtils;

namespace UnityEditor
{
    // Used for ShaderGraph Lit shaders
    class ShaderGraphLitGUI : BaseShaderGUI
    {
        public MaterialProperty workflowMode;

        MaterialProperty[] properties;

        // collect properties from the material properties
        public override void FindProperties(MaterialProperty[] properties)
        {
            // save off the list of all properties for shadergraph
            this.properties = properties;

            var material = materialEditor?.target as Material;
            if (material == null)
                return;

            base.FindProperties(properties);
            workflowMode = BaseShaderGUI.FindProperty(Property.SpecularWorkflowMode, properties, false);
        }

        public static void UpdateMaterial(Material material, MaterialUpdateType updateType)
        {
            // newly created materials should initialize the globalIlluminationFlags (default is off)
            if (updateType == MaterialUpdateType.CreatedNewMaterial)
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;

            // Determine whether render queue is user specified, or automatic.
            bool automaticRenderQueue = (material.HasProperty(Property.QueueControl) && material.GetInt(Property.QueueControl) == 0);
            if (automaticRenderQueue)
            {
                // Queue control is set to automatic
                // If the surface type property is not set (i.e., no material override)
                // Set the render queue back to "from shader"
                if (!material.HasProperty(Property.SurfaceType))
                    material.renderQueue = -1;
            }

            BaseShaderGUI.UpdateMaterialSurfaceOptions(material, automaticRenderQueue);
            LitGUI.SetupSpecularWorkflowKeyword(material, out bool isSpecularWorkflow);
        }

        public override void ValidateMaterial(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            UpdateMaterial(material, MaterialUpdateType.ModifiedMaterial);
        }

        public override void DrawSurfaceOptions(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            // Use default labelWidth
            EditorGUIUtility.labelWidth = 0f;

            // Detect any changes to the material
            if (workflowMode != null)
                DoPopup(LitGUI.Styles.workflowModeText, workflowMode, Enum.GetNames(typeof(LitGUI.WorkflowMode)));
            base.DrawSurfaceOptions(material);
        }

        // material main surface inputs
        public override void DrawSurfaceInputs(Material material)
        {
            DrawShaderGraphProperties(material, properties);
        }

        public override void DrawAdvancedOptions(Material material)
        {
            materialEditor.RenderQueueField();
            DoPopup(Styles.queueControl, queueControlProp, Styles.queueControlNames);
            base.DrawAdvancedOptions(material);

            // ignore emission color for shadergraphs, because shadergraphs don't have a hard-coded emission property, it's up to the user
            materialEditor.DoubleSidedGIField();
            materialEditor.LightmapEmissionFlagsProperty(0, enabled: true, ignoreEmissionColor: true);
        }
    }
} // namespace UnityEditor
