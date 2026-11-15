using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// "journal updated" card that slides in from the right
public class JournalToast : MonoBehaviour
{
    public RectTransform card;          // the sliding panel
    public TextMeshProUGUI headingText;  // "Journal updated"
    public TextMeshProUGUI lineText;     // entry name
    public float slideTime = 0.35f;
    public float showTime = 2.5f;
    public float hiddenOffset = 600f;   // how far right it hides
    public int maxWaiting = 3;          // more than this get bundled into one card

    private readonly Queue<(string heading, string line)> waiting = new Queue<(string, string)>();
    private Vector2 shownPos;
    private bool running;

    void Awake()
    {
        shownPos = card.anchoredPosition;
        card.gameObject.SetActive(false);
    }

    public void Show(string heading, string line)
    {
        waiting.Enqueue((heading, line));
        if (!running) StartCoroutine(Play());
    }

    IEnumerator Play()
    {
        running = true;
        card.gameObject.SetActive(true);

        while (waiting.Count > 0)
        {
            // loads at once, one card for all of them
            if (waiting.Count > maxWaiting)
            {
                headingText.text = "Journal updated";
                lineText.text = waiting.Count + " new entries";
                waiting.Clear();
            }
            else
            {
                (string heading, string line) next = waiting.Dequeue();
                headingText.text = next.heading;
                lineText.text = next.line;
            }

            yield return Slide(hiddenOffset, 0f);
            yield return new WaitForSecondsRealtime(showTime);
            yield return Slide(0f, hiddenOffset);
        }

        card.gameObject.SetActive(false);
        running = false;
    }

    // unscaled so it still moves while the journal pauses the game
    IEnumerator Slide(float from, float to)
    {
        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / slideTime)
        {
            float x = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));
            card.anchoredPosition = shownPos + new Vector2(x, 0f);
            yield return null;
        }
        card.anchoredPosition = shownPos + new Vector2(to, 0f);
    }
}