outlinkmeta
===========

Outlinkmeta is a plugin for Nutch 1.8. Its purpose is adding metadata to outlinks and making the metadata available in the NuchDocument that gets indexed.
The plugin is intended to be used in combination with custom parsers extracting this metadata. 
For instance, the [extractor plugin](https://github.com/BayanGroup/nutch-custom-search "nutch-custom-search") could be used for this task.

Example
-------

A document D1 (e.g., an HTML page) links to a document D2 (e.g., a PDF file) and contains information about it (title and authors, for instance).
After extracting this information and the link from D1, the plugin adds it to the outlink and D2 can be indexed including the metadata from D1.


Configuration
-------------

```xml
  <property>
    <name>outlinkmeta.urlField</name>
    <value></value>
    <description>Key of the parse metadata entry containing an URL that should be annotated.</description>
  </property>
  <property>
    <name>outlinkmeta.urlDescription</name>
    <value></value>
    <description>An anchor description for the outlink created from the above URL.</description>
  </property>
  <property>
    <name>outlinkmeta.fields</name>
    <value></value>
    <description>comma-separated list of parse metadata fields that should be put into the outlink's metadata.</description>
  </property>
  <property>
    <name>outlinkmeta.indexBinary</name>
    <value>false</value>
    <description>If true, the Base64 encoded binary content of the fetched document for the above URL will be made available in the NutchDocument that is send to the indexer.</description>
  </property>
```

Available fields
----------------

The NutchDocument contains all fields from the list in `outlinkmeta.fields` where the value of the respective parse metadata entry was not empty. 
Additionally, an entry with key `outlinkmeta.base64` can be present storing the Base64 encoded binary content of the original document.
