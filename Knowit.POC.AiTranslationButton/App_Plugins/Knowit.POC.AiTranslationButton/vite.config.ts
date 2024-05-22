import { defineConfig } from "vite";

export default defineConfig({
    build: {
        lib: {
            entry: "src/ai-translation-button-action.ts",
            formats: ["es"],
        },
        outDir: "dist",
        sourcemap: true,
        rollupOptions: {
            external: [/^@umbraco/],
        },
    },
});