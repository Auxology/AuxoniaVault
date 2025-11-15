import { GeistSans } from "geist/font/sans";
import type { Metadata } from "next";
import "./globals.css";
import { Providers } from "@/features/providers/providers";

export const metadata: Metadata = {
  title: "AuxoniaVault",
  description: "Fast. Simple. Secure.",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en" suppressHydrationWarning>
      <body
        className={`${GeistSans.className} antialiased`}
        suppressHydrationWarning
      >
        <Providers>{children}</Providers>
      </body>
    </html>
  );
}
