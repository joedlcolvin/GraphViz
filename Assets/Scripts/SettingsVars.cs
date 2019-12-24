using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class SettingsVars : MonoBehaviour
{
	bool original = false;

	GameObject mainMenu;

	public int numNodes = 100;
	public int totalInteractions = 10000;
	public int interactionType = 2;
	public int arrangeIterations = 50;
	public float opinionRadius = 0.6f;
	public float deltaOp = 0.01f;
	public float vizRadius = 10f;

	Slider numNodesSlider;
	Slider totalInteractionsSlider;
	Slider interactionTypeSlider;
	Slider opinionRadiusSlider;
	Slider deltaOpSlider;
	Slider vizRadiusSlider;

	Text numNodesValueText;
	Text totalInteractionsValueText;
	Text interactionTypeValueText;
	Text opinionRadiusValueText;
	Text deltaOpValueText;
	Text vizRadiusValueText;

	void Start()	
	{
		// Search for old settings
		if (GameObject.FindGameObjectsWithTag("Settings").Length > 1 && !original)
		{
			Destroy(this.gameObject);
		}
		else
		{
			SceneManager.sceneLoaded += OnSceneLoaded;
			OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
		}
	}

	public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.name == "MainMenu")
		{
			mainMenu = GameObject.Find("Main Menu");
			original = true;
			DontDestroyOnLoad(this.gameObject);
			mainMenu.GetComponent<MainMenuScript>().activatedSettingsMenu.AddListener(OnActiveSettingsMenu);
		}
	}

	public void OnActiveSettingsMenu()
	{
		// Find sliders
		numNodesSlider = GameObject.Find("numNodes").GetComponent<Slider>();
		totalInteractionsSlider = GameObject.Find("totalInteractions").GetComponent<Slider>();
		interactionTypeSlider = GameObject.Find("interactionType").GetComponent<Slider>();
		opinionRadiusSlider = GameObject.Find("opinionRadius").GetComponent<Slider>();
		deltaOpSlider = GameObject.Find("deltaOp").GetComponent<Slider>();
		vizRadiusSlider = GameObject.Find("vizRadius").GetComponent<Slider>();

		// Find slider text
		numNodesValueText = GameObject.Find("numNodesValueText").GetComponent<Text>();
		totalInteractionsValueText = GameObject.Find("totalInteractionsValueText").GetComponent<Text>();
		interactionTypeValueText = GameObject.Find("interactionTypeValueText").GetComponent<Text>();
		opinionRadiusValueText = GameObject.Find("opinionRadiusValueText").GetComponent<Text>();
		deltaOpValueText = GameObject.Find("deltaOpValueText").GetComponent<Text>();
		vizRadiusValueText = GameObject.Find("vizRadiusValueText").GetComponent<Text>();

		// Set sliders
		numNodesSlider.value = numNodes;
		totalInteractionsSlider.value = totalInteractions;
		interactionTypeSlider.value = interactionType;
		opinionRadiusSlider.value = opinionRadius;
		deltaOpSlider.value = deltaOp;
		vizRadiusSlider.value = vizRadius;

		// Set text
		numNodesValueText.text = numNodes.ToString();
		totalInteractionsValueText.text =totalInteractions.ToString();
		interactionTypeValueText.text =interactionType.ToString();
		opinionRadiusValueText.text =opinionRadius.ToString("0.00");
		deltaOpValueText.text =deltaOp.ToString("0.00");
		vizRadiusValueText.text =vizRadius.ToString("0.00");

		// Set listeners
		numNodesSlider.onValueChanged.AddListener(delegate {SetNumNodes(); });
		totalInteractionsSlider.onValueChanged.AddListener(delegate {SetTotalInteractions(); });
		interactionTypeSlider.onValueChanged.AddListener(delegate {SetInteractionType(); });
		opinionRadiusSlider.onValueChanged.AddListener(delegate {SetOpinionRadius(); });
		deltaOpSlider.onValueChanged.AddListener(delegate {SetDeltaOp(); });
		vizRadiusSlider.onValueChanged.AddListener(delegate {SetVizRadius(); });
	}

	public void SetNumNodes()
	{
		numNodes = (int)numNodesSlider.value;
		numNodesValueText.text = numNodes.ToString();
	}

	public void SetOpinionRadius()
	{
		opinionRadius = opinionRadiusSlider.value;
		opinionRadiusValueText.text =opinionRadius.ToString("0.000");
	}

	public void SetDeltaOp()
	{
		deltaOp = deltaOpSlider.value;
		deltaOpValueText.text =deltaOp.ToString("0.000");
	}

	public void SetTotalInteractions()
	{
		totalInteractions = (int)totalInteractionsSlider.value;
		totalInteractionsValueText.text =totalInteractions.ToString();
	}

	public void SetVizRadius()
	{
		vizRadius = vizRadiusSlider.value;
		vizRadiusValueText.text =vizRadius.ToString("0.000");
	}

	public void SetInteractionType()
	{
		interactionType = (int)interactionTypeSlider.value;
		interactionTypeValueText.text =interactionType.ToString();
	}
}
