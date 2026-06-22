import { DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MonoTypeOperatorFunction } from 'rxjs';

/**
 * A custom RxJS operator wrapper around Angular's takeUntilDestroyed.
 * Can be used inside an injection context without passing DestroyRef,
 * or outside an injection context by passing the injected DestroyRef.
 */
export function untilDestroyed<T>(destroyRef?: DestroyRef): MonoTypeOperatorFunction<T> {
  try {
    const ref = destroyRef || inject(DestroyRef);
    return takeUntilDestroyed<T>(ref);
  } catch (e) {
    if (!destroyRef) {
      throw new Error(
        'untilDestroyed operator must be called within an injection context, ' +
        'or you must pass a DestroyRef instance explicitly.'
      );
    }
    return takeUntilDestroyed<T>(destroyRef);
  }
}
