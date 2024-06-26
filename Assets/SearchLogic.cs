using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using System;
using UnityEngine.UI;

public class SearchLogic : MonoBehaviour
{
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject togglePrefab;
    [SerializeField] private GameObject resultPrefab;

    public static List<Book> books;
    public static List<string> skippable;

    private TMP_Dropdown book_select;
    private TMP_Dropdown chapter_select;
    private TMP_Dropdown start_verse_select;
    private TMP_Dropdown end_verse_select;

    public static string referenceDirectory = "Assets/reference/";

    // Start is called before the first frame update
    void Start()
    {
        books = new List<Book>();
        skippable = new List<string>();

        book_select = StaticData.findDeepChild(menu.transform, "Book").GetComponent<TMP_Dropdown>();
        chapter_select = StaticData.findDeepChild(menu.transform, "Chapter").GetComponent<TMP_Dropdown>();
        start_verse_select = StaticData.findDeepChild(menu.transform, "StartVerse").GetComponent<TMP_Dropdown>();
        end_verse_select = StaticData.findDeepChild(menu.transform, "EndVerse").GetComponent<TMP_Dropdown>();

        string[] files = Directory.GetFiles(referenceDirectory);
        foreach (string file in files)
        {
            if (file.EndsWith(".txt"))
            {
                string[] lines = File.ReadAllLines(file);
                Book book = new Book(file.Replace(".txt", "").Replace(referenceDirectory, ""));
                books.Add(book);
                foreach (string verse in lines)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(verse))
                        {
                            continue;
                        }
                        string[] wordsAndVerse = verse.Split(" ");
                        string[] index = wordsAndVerse[0].Split(":");
                        string[] words = new string[wordsAndVerse.Length - 1];
                        for (int q = 1; q < wordsAndVerse.Length; q++)
                        {
                            words[q - 1] = wordsAndVerse[q];
                        }
                        Verse finalVerse = new Verse(int.Parse(index[0]), int.Parse(index[1]), book, words);
                        book.addVerse(finalVerse);
                    } catch (Exception ex)
                    {
                        Debug.Log(ex);
                        Debug.Log(verse);
                    }
                }
                TMP_Dropdown.OptionData opt = new TMP_Dropdown.OptionData();
                opt.text = book.getName();
                book_select.options.Add(opt);

                GameObject bookToggle = Instantiate(togglePrefab);
                StaticData.findDeepChild(bookToggle.transform, "Label").GetComponent<TextMeshProUGUI>().text
                    = book.getName();
                bookToggle.transform.SetParent(StaticData.findDeepChild(menu.transform, "ToggleContent"));
            }
        }
    }

    public void setBook(int idx)
    {
        if (idx == 0)
        {
            chapter_select.interactable = false;
            start_verse_select.interactable = false;
            end_verse_select.interactable = false;
            return;
        }
        chapter_select.interactable = true;
        start_verse_select.interactable = true;
        end_verse_select.interactable = true;

        Book book = books[idx - 1];
        int numChapters = book.getNumChapters();
        int currentChapter = Mathf.Min(numChapters - 1, chapter_select.value);
        chapter_select.options.Clear();
        for (int q = 0; q < numChapters; q++)
        {
            TMP_Dropdown.OptionData opt = new TMP_Dropdown.OptionData();
            opt.text = (q + 1) + "";
            chapter_select.options.Add(opt);
        }
        chapter_select.value = currentChapter;
    }

    public void setChapter(int idx)
    {
        int numVerses = books[book_select.value - 1]
            .getNumVersesInChapter(idx);
        int currentStartVerse = Mathf.Min(numVerses - 1, start_verse_select.value);
        int currentEndVerse = Mathf.Min(currentStartVerse, end_verse_select.value);

        start_verse_select.options.Clear();
        end_verse_select.options.Clear();

        for (int q = 0; q < numVerses; q++)
        {
            TMP_Dropdown.OptionData opt = new TMP_Dropdown.OptionData();
            opt.text = (q + 1) + "";
            start_verse_select.options.Add(opt);
            end_verse_select.options.Add(opt);
        }

        start_verse_select.value = currentStartVerse;
        end_verse_select.value = currentEndVerse;
    }

    public void setStartVerse(int idx)
    {
        end_verse_select.value = Mathf.Max(idx, end_verse_select.value);
    }

    public void setEndVerse(int idx)
    {
        start_verse_select.value = Mathf.Min(idx, start_verse_select.value);
    }

    public void search()
    {
        Book book = books[book_select.value - 1];
        List<Verse> chapter = book.getChapter(chapter_select.value);
        List<Dictionary<string, int>> verses = new List<Dictionary<string, int>>();
        Dictionary<Verse, int> scores = new Dictionary<Verse, int>();
        for (int q = start_verse_select.value; q <= end_verse_select.value; q++)
        {
            verses.Add(chapter[q].getWords());
        }

        int childCount = StaticData.findDeepChild(menu.transform, "ToggleContent").childCount;
        for (int q = 0; q < childCount; q++)
        {
            if (StaticData.findDeepChild(menu.transform, "ToggleContent").GetChild(q)
                .GetComponent<Toggle>().isOn)
            {
                Book searchBook = books[q];
                for (int w = 0; w < searchBook.getNumChapters(); w++)
                {
                    foreach (Verse v in searchBook.getChapter(w))
                    {
                        scores.Add(v, v.getSimilarityScore(verses));
                    }
                }
            }
        }

        List<Verse> verseTexts = new List<Verse>(scores.Keys);
        radixsort(verseTexts, scores);

        for (int q = verseTexts.Count - 1; q >= 0; q--)
        {
            GameObject result = Instantiate(resultPrefab);
            StaticData.findDeepChild(result.transform, "VerseName").GetComponent<TextMeshProUGUI>()
                .text = verseTexts[q].getReference();
            result.transform.SetParent(StaticData.findDeepChild(menu.transform, "ResultsContent"));
        }
    }

    public static int getMax(List<Verse> arr, Dictionary<Verse, int> dict)
    {
        int mx = dict[arr[0]];
        for (int i = 1; i < arr.Count; i++)
        {
            if (dict[arr[i]] > mx)
            {
                mx = dict[arr[i]];
            }
        }
        return mx;
    }

    // A function to do counting sort of arr[] according to
    // the digit represented by exp.
    public static void countSort(List<Verse> arr, Dictionary<Verse, int> dict, int exp)
    {
        Verse[] output = new Verse[arr.Count]; // output array
        int i;
        int[] count = new int[10];

        // initializing all elements of count to 0
        for (i = 0; i < 10; i++)
        {
            count[i] = 0;
        }

        // Store count of occurrences in count[]
        for (i = 0; i < arr.Count; i++)
        {
            count[(dict[arr[i]] / exp) % 10]++;
        }

        // Change count[i] so that count[i] now contains
        // actual
        //  position of this digit in output[]
        for (i = 1; i < 10; i++)
        {
            count[i] += count[i - 1];
        }

        // Build the output array
        for (i = arr.Count - 1; i >= 0; i--)
        {
            output[count[(dict[arr[i]] / exp) % 10] - 1] = arr[i];
            count[(dict[arr[i]] / exp) % 10]--;
        }

        // Copy the output array to arr[], so that arr[] now
        // contains sorted numbers according to current
        // digit
        for (i = 0; i < arr.Count; i++)
        {
            arr[i] = output[i];
        }
    }

    // The main function to that sorts arr[] of size n using
    // Radix Sort
    public static void radixsort(List<Verse> arr, Dictionary<Verse, int> dict)
    {
        // Find the maximum number to know number of digits
        int m = getMax(arr, dict);

        // Do counting sort for every digit. Note that
        // instead of passing digit number, exp is passed.
        // exp is 10^i where i is current digit number
        for (int exp = 1; m / exp > 0; exp *= 10)
        {
            countSort(arr, dict, exp);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
