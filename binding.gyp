{
    'targets' : [
        {
            'target_name' : 'osu_difficulty_module',
            'include_dirs' : [
                "<!(node -e \"require('nan')\")",
            ],
            'conditions' : [
                ['OS=="win"', {
                    'libraries': [
                        
                    ],
                }],
                ['OS=="mac"', {
                    'libraries': [
                        
                    ],
                }],
            ],
            'cflags_cc': [
                '-std=c++11'
            ],
            'sources' : [
                "src/init.cc"
            ],
			"msbuild_settings": {
				"ClCompile": {
					"CompileAsManaged": "true",
					"ExceptionHandling": "Async"
				},
			},
        }
    ]
}