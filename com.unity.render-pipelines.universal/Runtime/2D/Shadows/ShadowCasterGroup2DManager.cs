using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Experimental.Rendering.Universal
{
    internal class ShadowCasterGroup2DManager
    {

        static List<ShadowCasterGroup2D> m_ShadowCasterGroups = null;

        public static List<ShadowCasterGroup2D> shadowCasterGroups { get { return m_ShadowCasterGroups; } }

        public static void AddGroup(ShadowCasterGroup2D group)
        {
            if (group == null)
                return;

            if (m_ShadowCasterGroups == null)
                m_ShadowCasterGroups = new List<ShadowCasterGroup2D>();

            LightUtility.AddShadowCasterGroupToList(group, m_ShadowCasterGroups);
        }
        public static void RemoveGroup(ShadowCasterGroup2D group)
        {
            if (group != null && m_ShadowCasterGroups != null)
                LightUtility.RemoveShadowCasterFromList(group, m_ShadowCasterGroups);
        }


    }
}
