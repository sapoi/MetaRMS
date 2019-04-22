namespace SharedLibrary.StaticFiles
{
    static class ApplicationDescriptorJSONSchema
    {
        //TODO move this into a file
        /// <summary>
        /// This method returns JSON schema to validate application descriptor against.
        /// </summary>
        /// <returns>JSON schema</returns>
        public static string GetSchema()
        {
            return @"
{
	'$schema': 'http://json-schema.org/draft-07/schema#',
    '$id': 'http://sapoi.aspifyhost.com/jsonschema.json',
    'title': 'MetaRMS application descriptor schema',
    'description': 'A JSON schema of a string describing one application for MetaRMS application.',

    'definitions': {
        'attribute': {
            'type': 'object',
            'properties': {
                'Name': {
                    'type': 'string'
                },
                'Description': {
                    'type': ['string', 'null']
                },
                'Type': {
                    'type': 'string'
                },
                'OnDeleteAction': {
                    'type': 'string',
                    'enum': ['cascade', 'setEmpty', 'protect']
                },
                'Required': {
                    'type': ['boolean', 'null']
                },
                'Unique': {
                    'type': ['boolean', 'null']
                },
                'Safer': {
                    'type': ['boolean', 'null']
                },
                'Min': {
                    'type': ['integer', 'null']
                },
                'Max': {
                    'type': ['integer', 'null']
                }
            },
            'required': ['Name', 'Type'],
            'additionalProperties': false
        }
    },

    'type': 'object',
    'properties': {
        'ApplicationName': {
            'type': 'string'
        },
        'LoginApplicationName': {
            'type': 'string'
        },
        'DefaultLanguage': {
            'type': 'string',
            'enum': ['en']
        },
        'SystemDatasets': {
            'type': 'object',
            'properties': {
                'UsersDatasetDescriptor': {
                    'type': 'object',
                    'properties': {
                        'Name': {
                            'type': 'string'
                        },
                        'Description': {
                            'type': ['string', 'null']
                        },
                        'PasswordAttribute': {
                            '$ref': '#/definitions/attribute'
                        },
                        'Attributes': {
                            'type': 'array',
                            'minItems': 1,
                            'uniqueItems': true,
                            'items': {
                                '$ref': '#/definitions/attribute'
                            }
                        }
                    },
                    'required': ['Name', 'PasswordAttribute', 'Attributes'],
                    'additionalProperties': false
                }
            },
            'required': ['UsersDatasetDescriptor'],
            'additionalProperties': false
        },
        'Datasets': {
            'type': 'array',
            'minItems': 1,
            'uniqueItems': true,
            'items': {
                'type': 'object',
                'properties': {
                    'Name': {
                        'type': 'string'
                    },
                    'Description': {
                        'type': ['string', 'null']
                    },
                    'Attributes': {
                        'type': 'array',
                        'minItems': 1,
                        'uniqueItems': true,
                        'items': {
                            '$ref': '#/definitions/attribute'
                        }
                    }
                },
                'required': ['Name', 'Attributes'],
                'additionalProperties': false
            }
        }
    },
    'required': ['ApplicationName', 'LoginApplicationName', 'DefaultLanguage', 'SystemDatasets', 'Datasets'],
    'additionalProperties': false
}
            ";
        }
        
    }
}