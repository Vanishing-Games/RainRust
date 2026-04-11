using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    public class VgLoadingProgressBar : MonoProgressable
    {
        public override void Hide()
        {
            gameObject.SetActive(false);
            if (m_Material != null)
            {
                Destroy(m_Material);
                m_Material = null;
            }
        }

        public override void Show()
        {
            gameObject.SetActive(true);
            var image = GetComponent<Image>();
            if (image != null && image.material != null)
            {
                // Create a material instance to avoid modifying the asset
                m_Material = new Material(image.material);
                image.material = m_Material;
                UpdateProgress(0f);
            }
        }

        public override void UpdateProgress(float progress)
        {
            if (m_Material != null)
            {
                m_Material.SetFloat("_Fill", progress);
            }
        }

        private Material m_Material;
    }
}
