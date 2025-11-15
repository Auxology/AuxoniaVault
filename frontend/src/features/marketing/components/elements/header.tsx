"use client";

import { TextAlignEnd, X } from "@aliimam/icons";
import Image from "next/image";
import Link from "next/link";
import { useState } from "react";
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogOverlay,
  DialogPortal,
  DialogTitle,
} from "@/components/animate-ui/primitives/radix/dialog";
import { Button, buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";

const navigation = [
  { name: "Pricing", href: "#" },
  { name: "Our Mission", href: "#" },
  { name: "Your Agreement", href: "#" },
  { name: "Contact", href: "#" },
];
export function Header() {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

  return (
    <header>
      <nav
        aria-label="Global"
        className="mx-auto flex max-w-7xl items-center justify-between gap-x-6 p-6 lg:px-8"
      >
        <div className="flex lg:flex-1">
          <Link href="/" className="-m-1.5 p-1.5">
            <span className="sr-only">AuxoniaVault</span>
            <Image
              alt="AuxoniaVault"
              src="/auxoniavault_light.svg"
              width={42}
              height={42}
              quality={100}
            />
          </Link>
        </div>
        <div className="hidden lg:flex lg:gap-x-12">
          {navigation.map((item) => (
            <Link
              key={item.name}
              href={item.href}
              className="text-sm/6 text-muted-foreground hover:text-foreground"
            >
              {item.name}
            </Link>
          ))}
        </div>
        <div className="hidden lg:flex flex-1 items-center justify-end gap-x-6">
          <Link
            href="/sign-up"
            className={cn(
              buttonVariants({ variant: "default", size: "sm" }),
              "",
            )}
          >
            Get Started
          </Link>
          <Link
            href="/login"
            className={cn(buttonVariants({ variant: "outline", size: "sm" }))}
          >
            Login
          </Link>
        </div>
        <div className="flex lg:hidden">
          <Button
            type="button"
            onClick={() => setMobileMenuOpen(true)}
            variant="ghost"
            size="icon"
          >
            <span className="sr-only">Open main menu</span>
            <TextAlignEnd strokeWidth={2} className="size-6 text-foreground" />
          </Button>
        </div>
        <Dialog open={mobileMenuOpen} onOpenChange={setMobileMenuOpen}>
          <DialogPortal>
            <DialogOverlay className="fixed inset-0" />
            <DialogContent
              from="right"
              className="fixed inset-y-0 right-0 w-full overflow-y-auto bg-background px-6 py-6 sm:max-w-sm sm:ring-1 sm:ring-border lg:hidden"
            >
              <div className="flex items-center justify-between">
                <DialogTitle className="text-lg font-semibold">
                  AuxoniaVault
                </DialogTitle>
                <DialogClose asChild>
                  <Button type="button" variant="ghost" size="icon">
                    <span className="sr-only">Close menu</span>
                    <X strokeWidth={2} className="size-6" aria-hidden="true" />
                  </Button>
                </DialogClose>
              </div>

              <div className="mt-6 flow-root">
                <div className="-my-6 divide-y divide-border">
                  <div className="space-y-2 py-6">
                    {navigation.map((item) => (
                      <Link
                        key={item.name}
                        href={item.href}
                        onClick={() => setMobileMenuOpen(false)}
                        className="-mx-3 block rounded-lg px-3 py-2 text-sm text-muted-foreground"
                      >
                        {item.name}
                      </Link>
                    ))}
                  </div>
                  <div className="flex flex-col space-y-2 py-6">
                    <Link
                      href="/sign-up"
                      onClick={() => setMobileMenuOpen(false)}
                      className={cn(buttonVariants({ variant: "default" }))}
                    >
                      Get Started
                    </Link>
                    <Link
                      href="/login"
                      onClick={() => setMobileMenuOpen(false)}
                      className={cn(buttonVariants({ variant: "outline" }))}
                    >
                      Login
                    </Link>
                  </div>
                </div>
              </div>
            </DialogContent>
          </DialogPortal>
        </Dialog>
      </nav>
    </header>
  );
}
