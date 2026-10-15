# Anonymous BSC complete-history retry

The same generated complete-history plan was tried against the anonymous provider that supplied pinned state. Chain identity 56 succeeded, but the first three log batches were rejected with a 60-compute-unit/minute limit. The runner stopped with zero credited historical batches and 4,252 remaining. Requests and explicit reset/retry metadata are preserved. The successful narrow state/receipt queries elsewhere are not complete log coverage. Resume using provider-compliant pacing; do not reinterpret rate-limit responses as empty history.
