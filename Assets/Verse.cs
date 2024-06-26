using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Verse
{
    private Book book;
    private int chapter;
    private int verseNumber;
    private string[] text;
    private Dictionary<string, int> words;

    public Verse(int chapter, int verseNumber, Book book, string[] text)
    {
        this.book = book;
        this.chapter = chapter;
        this.verseNumber = verseNumber;
        this.text = text;
        words = new Dictionary<string, int>();
        foreach (string word in text)
        {
            addWord(word);
        }
    }

    private void addWord(string word)
    {
        word = word.ToLower();
        if (words.ContainsKey(word))
        {
            words[word]++;
        }
        else
        {
            words.Add(word, 1);
        }
    }

    public string getReference()
    {
        return $"{book.getName()} {chapter}:{verseNumber}";
    }
    public string getText()
    {
        string ret = "";
        for (int q = 0; q < text.Length; q++)
        {
            ret += text[q] + " ";
        }
        return ret;
    }

    public int getFrequencyOfWord(string word)
    {
        if (words.ContainsKey(word))
        {
            return words[word];
        }
        return 0;
    }

    public int getSimilarityScore(List<Dictionary<string, int>> passage)
    {
        int score = 0;
        foreach (Dictionary<string, int> verse in passage)
        {
            foreach (string word in verse.Keys)
            {
                if (!SearchLogic.skippable.Contains(word) && words.ContainsKey(word))
                {
                    score += words[word];
                }
            }
        }
        return score;
    }
    public int getChapter()
    {
        return chapter;
    }
    public Dictionary<string, int> getWords()
    {
        return words;
    }
}
