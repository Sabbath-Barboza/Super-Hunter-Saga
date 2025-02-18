using UnityEngine;
using UnityEngine.UI;

public class RatingSystem : MonoBehaviour
{
    [Header("Image:")]
    [SerializeField] private GameObject[] YellowStarImage;

    [Header("GameObject:")]
    [SerializeField] private GameObject RatingPanel;
    [SerializeField] private GameObject ThankYouPanel;

    public Button[] WhiteStarButton;
    public Button SubmitButton;

    private bool HasRated;
    private int Rating = 0;

    private void Start()
    {
        // Load The HasRated State From PlayerPrefs
        HasRated = PlayerPrefs.GetInt("HasRated", 0) > 0;

        if (!HasRated && IsThirdEntry())
        {
            RatingPanel.SetActive(true);

            // Attach Listeners to the buttons
            for (int i = 0; i < WhiteStarButton.Length; i++)
            {
                int index = i; // Capture index For Closure
                WhiteStarButton[i].onClick.AddListener(() => OnStarClick(index));
            }

            // Attach Clicked to the Submit Button
            SubmitButton.onClick.AddListener(SubmitRating);

            // Initialize the Star Visuals
            UpdateStars();
        }
        else
        {
            RatingPanel.SetActive(false);
        }
    }

    private void UpdateStars()
    {
        // Update Star Visual based on the Current Rating
        for (int i = 0; i < WhiteStarButton.Length; i++)
        {
            YellowStarImage[i].SetActive(i < Rating);
        }
    }

    private void SubmitRating()
    {
        if (Rating > 3)
        {
            Application.OpenURL("https://play.google.com/store/apps/details?id=com.gameplmumbai.SuperHunterSaga");
        }
        else
        {
            Debug.Log("User rated less than or equal to 3 stars");
        }

        // Save Rating And Disable The Panel
        PlayerPrefs.SetInt("HasRated", Rating);
        PlayerPrefs.Save();

        HasRated = true;
        RatingPanel.SetActive(false);
        ThankYouPanel.SetActive(true);
    }

    private void OnStarClick(int index)
    {
        // Set Rating and Update Star
        Rating = Mathf.Clamp(index + 1, 1, WhiteStarButton.Length); // Ensure Valid range
        UpdateStars();
    }

    private bool IsThirdEntry()
    {
        int entryCount = PlayerPrefs.GetInt("EntryCount", 0);
        entryCount++;

        PlayerPrefs.SetInt("EntryCount", entryCount);
        PlayerPrefs.Save();

        return entryCount == 3; // return true only if this is the Third entry
    }
}
