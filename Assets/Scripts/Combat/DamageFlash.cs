using System.Collections;
using UnityEngine;

public class DamageFlash : MonoBehaviour
{
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashDuration = 0.12f;

    private Renderer[] renderers;
    private MaterialPropertyBlock[] originalBlocks;
    private Coroutine flashRoutine;

    private void Awake()
    {
        CacheRenderers();
    }

    private void CacheRenderers()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        originalBlocks = new MaterialPropertyBlock[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalBlocks[i] = new MaterialPropertyBlock();
            renderers[i].GetPropertyBlock(originalBlocks[i]);
        }
    }

    public void Flash()
    {
        if (!isActiveAndEnabled) return;

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        RestoreAppearance();
        // Rolling creates a visual child and equipping creates weapon models at
        // runtime. Include the current renderers without caching a red flash.
        CacheRenderers();
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;

            var block = new MaterialPropertyBlock();
            renderers[i].GetPropertyBlock(block);
            block.SetColor("_Color", flashColor);
            block.SetColor("_BaseColor", flashColor);
            renderers[i].SetPropertyBlock(block);
        }

        yield return new WaitForSeconds(flashDuration);

        RestoreAppearance();
        flashRoutine = null;
    }

    private void RestoreAppearance()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].SetPropertyBlock(originalBlocks[i]);
            }
        }
    }

    private void OnDisable()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }

        RestoreAppearance();
    }
}
