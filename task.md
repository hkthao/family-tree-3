Implement infinite scroll + pagination using React Native FlatList and Zustand.

Requirements:
1. Create a Zustand store named usePublicFamilyStore with:
   - families: array
   - page (number)
   - hasMore (boolean)
   - loading (boolean)
   - error (string or null)
   - reset() → clear data and reset page=1, hasMore=true
   - fetchFamilies({page, search}) async → call API:
       GET /api/public/families?page={page}&itemsPerPage=10&search={search}
     Append when page>1, replace when page=1.
     If API returns <10 items → hasMore = false.
     Prevent parallel requests: if loading=true → return early.

2. In FamilySearchScreen:
   - State: search text.
   - Use debounce 400ms before calling API.
   - useEffect(() => { reset(); fetchFamilies({page:1, search}); }, [debouncedSearch])
   - Infinite load:
       onEndReached → if (!loading && hasMore) fetchFamilies(page+1)
   - Pull to refresh:
       onRefresh → reset() + fetchFamilies(page=1)
   - ListFooterComponent → loading spinner when page>1
   - onEndReachedThreshold = 0.3

3. Ensure:
   - No duplicated API calls
   - No double onEndReached bug
   - No infinite loop
   - Works on iOS, Android, Expo

