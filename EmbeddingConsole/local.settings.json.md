# local.settings.json

This file is used to specify settings for the console application contained in this directory. Create a `local.settings.json` file and place it in the same directory with this file. The JSON in the file is formatted as described below. 

```json
{
	"embedding": {
		"model": {
			"endpoint": "<endpoint URI>",
			"name": "<Name of embedding model>"
			"key": "<API key>",
			"dimensions": <Number_of_dimensions>
		}
	}
}
```

