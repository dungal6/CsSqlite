fn main() {
    cc::Build::new().file("native/sqlite3.c").compile("sqlite3");

    bindgen::Builder::default()
        .header("native/sqlite3.h")
        .generate()
        .unwrap()
        .write_to_file("src/sqlite3.rs")
        .unwrap();

    let builder = csbindgen::Builder::default()
        .input_bindgen_file("src/sqlite3.rs")
        .method_filter(|x| !x.starts_with("sqlite3_win32"))
        .csharp_namespace("CsSqlite")
        .csharp_method_prefix("")
        .csharp_class_accessibility("public")
        .csharp_use_function_pointer(false)
        .csharp_dll_name("libsqlite3");
    builder
        .generate_to_file(
            "src/sqlite3_csbindgen.rs",
            "../CsSqlite.Native/NativeMethods.g.cs",
        )
        .unwrap();

    builder
        .csharp_dll_name_if("(UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR", "__Internal")
        .csharp_dll_name_if("UNITY_ANDROID && !UNITY_EDITOR", "sqliteX")
        .generate_csharp_file("../CsSqlite.Unity/Assets/CsSqlite.Unity/Native/NativeMethods.g.cs")
        .unwrap();
}
