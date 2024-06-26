using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Book
{
    private string bookName;
    private List<List<Verse>> verses;
    public Book(string bookName)
    {
        this.bookName = bookName;
        verses = new List<List<Verse>>();
    }
    public string getName()
    {
        return bookName;
    }
    public void addVerse(Verse verse)
    {
        while (verses.Count < verse.getChapter())
        {
            verses.Add(new List<Verse>());
        }
        verses[verse.getChapter() - 1].Add(verse);
    }

    public int getNumChapters()
    {
        return verses.Count;
    }
    public List<Verse> getChapter(int chapterIdx)
    {
        return verses[chapterIdx];
    }
    public int getNumVersesInChapter(int chapterIdx)
    {
        Debug.Log($"{getName()}, Chapter {chapterIdx} ({verses.Count})");
        return verses[chapterIdx].Count;
    }
}
