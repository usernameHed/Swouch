Just Play Around with the timeScale slider in play mode.

You may notice in bonus, that I use TimeEditor instead of Time to setup the timeScale.
If you use Editor scripts, or standard Monobehavior script, using [ExecuteInEditMode] with an Update. You may want to use TimeEditor.deltaTime to use the deltaTime, and use the timeScale in editor.