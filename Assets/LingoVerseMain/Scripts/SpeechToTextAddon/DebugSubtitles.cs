using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugSubtitles : MonoBehaviour
{
    // Subtitles settings
    public int maxCharCount = 250;
    
    // Subtitles buffer 
    // Subtitles buffer 
    private Queue<string> _textQueue;
    private TMP_Text _text;
    
    // Internals
    private int _prevQueueLength = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        _textQueue = new Queue<string>();
        _text = GetComponentInChildren<TMP_Text>();
    }

    private void Update()
    {
        if (_textQueue.Count > _prevQueueLength)
        {
            ShowNextSegment();
        }
    }

    public void AddSegment(string text)
    {
        if (text.Length > maxCharCount)
        {
            string[] segments = text.Split(' ');
            int halfCount = segments.Length / 2;
            _textQueue.Enqueue(String.Join(" ", segments.AsSpan(0, halfCount).ToArray()));
            _textQueue.Enqueue(String.Join(" ", segments.AsSpan(halfCount).ToArray()));
        }
        else
        {
            _textQueue.Enqueue(text);
        }
    }
    
    // Coroutine for serving subtitles
    private void ShowNextSegment()
    {
        if (_textQueue.Count > 0)
        {
            string text = _textQueue.Dequeue();
            _prevQueueLength--;
            // Update Shown Subtitles if any new text
            if (text.Length > 0)
                _text.text = text;
        }
    }
}
