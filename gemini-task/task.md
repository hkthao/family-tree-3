=> ERROR [frontend build 6/6] RUN npm run build                                                                                                                          8.3s
 => [face-service] resolving provenance for metadata file                                                                                                                 0.0s
------
 > [frontend build 6/6] RUN npm run build:
0.121 
0.121 > frontend@0.0.0 build
0.121 > run-p type-check "build-only {@}" --
0.121 
0.198 
0.198 > frontend@0.0.0 build-only
0.198 > vite build
0.198 
0.198 
0.198 > frontend@0.0.0 type-check
0.198 > vue-tsc --build
0.198 
0.485 vite v7.1.10 building for production...
0.520 transforming...
4.374 ✓ 1604 modules transformed.
4.542 node_modules/lottie-web/build/player/lottie.js (17010:32): Use of eval in "node_modules/lottie-web/build/player/lottie.js" is strongly discouraged as it poses security risks and may cause issues with minification.
4.624 rendering chunks...
4.909 computing gzip size...
4.943 dist/index.html                                                                             0.67 kB │ gzip:   0.36 kB
4.943 dist/assets/404-KybqypYR.png                                                               36.77 kB
4.943 dist/assets/materialdesignicons-webfont-Dp5v-WZN.woff2                                    403.22 kB
4.943 dist/assets/materialdesignicons-webfont-PXm3-2wK.woff                                     587.98 kB
4.943 dist/assets/materialdesignicons-webfont-B7mPwVP_.ttf                                    1,307.66 kB
4.943 dist/assets/materialdesignicons-webfont-CSr8KVlo.eot                                    1,307.88 kB
4.943 dist/assets/FaceResultList-BA6hgqEJ.css                                                     1.37 kB │ gzip:   0.53 kB
4.943 dist/assets/index-CzfsfvVk.css                                                            860.46 kB │ gzip: 123.43 kB
4.943 dist/assets/FamilyTreeView-9nKGFRH_.js                                                      0.06 kB │ gzip:   0.08 kB
4.943 dist/assets/event-type.d-Cn9-V6TT.js                                                        0.13 kB │ gzip:   0.12 kB
4.943 dist/assets/EventRouterView-cpFD9Aax.js                                                     0.25 kB │ gzip:   0.21 kB
4.943 dist/assets/MemberRouterView-DS2Zcsq0.js                                                    0.25 kB │ gzip:   0.21 kB
4.943 dist/assets/FamilyRouterView-CoqcoEou.js                                                    0.25 kB │ gzip:   0.21 kB
4.943 dist/assets/RelationshipRouterView-DbmQ5Mt4.js                                              0.25 kB │ gzip:   0.21 kB
4.943 dist/assets/relationshipTypes-BGH07gms.js                                                   0.47 kB │ gzip:   0.25 kB
4.943 dist/assets/GenderSelect.vue_vue_type_script_setup_true_lang-DQ6y2zRm.js                    0.59 kB │ gzip:   0.35 kB
4.943 dist/assets/FaceUploadInput-Oimx14GU.js                                                     0.80 kB │ gzip:   0.52 kB
4.943 dist/assets/SocialLogin-CC6R_VW5.js                                                         0.88 kB │ gzip:   0.39 kB
4.943 dist/assets/NotFoundView-eFet_IIF.js                                                        1.07 kB │ gzip:   0.57 kB
4.943 dist/assets/EventAddView-BTbRjnxe.js                                                        1.17 kB │ gzip:   0.61 kB
4.943 dist/assets/DateInputField.vue_vue_type_script_setup_true_lang-CMMHcL6x.js                  1.21 kB │ gzip:   0.64 kB
4.943 dist/assets/dateUtils-I2bz1Gjr.js                                                           1.23 kB │ gzip:   0.69 kB
4.943 dist/assets/FamilyAddView-DWvo1cFJ.js                                                       1.25 kB │ gzip:   0.65 kB
4.943 dist/assets/RelationshipAddView-D1ax-BBM.js                                                 1.34 kB │ gzip:   0.67 kB
4.943 dist/assets/MemberAddView-BI998DSh.js                                                       1.36 kB │ gzip:   0.69 kB
4.944 dist/assets/RelationshipEditView-9H9iOoz2.js                                                1.61 kB │ gzip:   0.78 kB
4.944 dist/assets/FamilyEditView-B37v9Wtc.js                                                      1.61 kB │ gzip:   0.81 kB
4.944 dist/assets/MemberEditView-m8PdM_an.js                                                      1.65 kB │ gzip:   0.81 kB
4.944 dist/assets/FaceResultList.vue_vue_type_style_index_0_scoped_c8988bcc_lang-DOn804a8.js      1.69 kB │ gzip:   0.68 kB
4.944 dist/assets/EventEditView-BJeNna-s.js                                                       1.74 kB │ gzip:   0.85 kB
4.944 dist/assets/FaceUploadView-O8Db3pix.js                                                      1.78 kB │ gzip:   0.82 kB
4.944 dist/assets/EventDetailView-C7xifRfA.js                                                     1.96 kB │ gzip:   0.93 kB
4.944 dist/assets/RelationshipDetailView-CILAnDa_.js                                              1.98 kB │ gzip:   0.93 kB
4.944 dist/assets/RelationshipForm.vue_vue_type_script_setup_true_lang-BCfYX9f3.js                2.15 kB │ gzip:   0.82 kB
4.944 dist/assets/MemberAutocomplete.vue_vue_type_script_setup_true_lang-BCZ6C_8v.js              2.23 kB │ gzip:   1.09 kB
4.944 dist/assets/FaceSearchView-k-86SftR.js                                                      2.44 kB │ gzip:   1.09 kB
4.944 dist/assets/FamilyDetailView-DHSHyD7a.js                                                    2.58 kB │ gzip:   1.04 kB
4.944 dist/assets/event.store-6kybYHsU.js                                                         2.78 kB │ gzip:   0.83 kB
4.944 dist/assets/naturalLanguageInput.store-CLgozTWm.js                                          2.79 kB │ gzip:   0.76 kB
4.944 dist/assets/LoginView-CX4iFwPe.js                                                           3.11 kB │ gzip:   1.29 kB
4.944 dist/assets/RegisterView-DjivZTV1.js                                                        3.20 kB │ gzip:   1.35 kB
4.944 dist/assets/FamilyForm.vue_vue_type_script_setup_true_lang-CeZ4XOn2.js                      3.77 kB │ gzip:   1.36 kB
4.944 dist/assets/EventForm.vue_vue_type_script_setup_true_lang-bi9vPo5t.js                       3.79 kB │ gzip:   1.12 kB
4.944 dist/assets/MemberDetailView-CxcUUeom.js                                                    3.83 kB │ gzip:   1.44 kB
4.944 dist/assets/FaceDetectionSidebar-DF0RKZWv.js                                                3.97 kB │ gzip:   1.61 kB
4.944 dist/assets/MemberForm.vue_vue_type_script_setup_true_lang-Bb2HCwlM.js                      4.51 kB │ gzip:   1.20 kB
4.944 dist/assets/AvatarInput-C-hEmgUG.js                                                         4.52 kB │ gzip:   1.93 kB
4.944 dist/assets/EventCalendar.vue_vue_type_script_setup_true_lang-6I1rH4Dr.js                   4.65 kB │ gzip:   1.96 kB
4.944 dist/assets/FaceLabelingView-IBq0sBnM.js                                                    5.11 kB │ gzip:   1.89 kB
4.944 dist/assets/UserSettingsPage-BBfHjnsP.js                                                    6.36 kB │ gzip:   1.64 kB
4.944 dist/assets/AIBiographyGeneratorView-Dy8XUYqT.js                                            7.27 kB │ gzip:   2.26 kB
4.944 dist/assets/DashboardView-CG9jqqwB.js                                                       9.22 kB │ gzip:   2.54 kB
4.944 dist/assets/ChunkAdmin-CLQKxJkb.js                                                          9.37 kB │ gzip:   3.17 kB
4.944 dist/assets/RelationshipListView-C9wxGTcy.js                                               10.83 kB │ gzip:   3.69 kB
4.944 dist/assets/FamilyListView-wQ_wR0pF.js                                                     11.96 kB │ gzip:   4.28 kB
4.944 dist/assets/MemberListView-Cqn5i1HM.js                                                     12.04 kB │ gzip:   4.31 kB
4.944 dist/assets/EventListView--vyzAJzE.js                                                      12.37 kB │ gzip:   4.37 kB
4.944 dist/assets/index-C0ScZ7lI.js                                                           1,869.27 kB │ gzip: 505.25 kB
4.944 
4.944 (!) Some chunks are larger than 500 kB after minification. Consider:
4.944 - Using dynamic import() to code-split the application
4.944 - Use build.rollupOptions.output.manualChunks to improve chunking: https://rollupjs.org/configuration-options/#output-manualchunks
4.944 - Adjust chunk size limit for this warning via build.chunkSizeWarningLimit.
4.944 ✓ built in 4.45s
6.367 src/components/aiBiography/AIBiographyInputPanel.vue(10,28): error TS2345: Argument of type 'Date | undefined' is not assignable to parameter of type 'string | null | undefined'.
6.367   Type 'Date' is not assignable to type 'string'.
6.367 src/components/face/FaceBoundingBoxViewer.vue(43,37): error TS2304: Cannot find name 'PropType'.
6.367 src/components/face/FaceDetectionSidebar.vue(48,37): error TS2304: Cannot find name 'PropType'.
6.367 src/components/face/FaceLabelCard.vue(97,45): error TS2769: No overload matches this call.
6.367   Overload 2 of 2, '(options: WritableComputedOptions<Member[], Member[]>, debugOptions?: DebuggerOptions | undefined): WritableComputedRef<Member[], Member[]>', gave the following error.
6.367     Argument of type '() => { id: string; fullName: string; avatarUrl: string; }[]' is not assignable to parameter of type 'WritableComputedOptions<Member[], Member[]>'.
6.367 src/services/aiBiography/mock.aiBiography.service.ts(22,7): error TS2353: Object literal may only specify known properties, and 'provider' does not exist in type 'BiographyResultDto'.
6.367 src/services/member/mock.member.service.ts(345,14): error TS2420: Class 'MockMemberService' incorrectly implements interface 'IMemberService'.
6.367   Property 'updateMemberBiography' is missing in type 'MockMemberService' but required in type 'IMemberService'.
6.367 src/services/relationship/mock.relationship.service.ts(4,14): error TS2420: Class 'MockRelationshipService' incorrectly implements interface 'IRelationshipService'.
6.367   Property 'addItems' is missing in type 'MockRelationshipService' but required in type 'IRelationshipService'.
6.367 src/services/service.factory.ts(72,5): error TS2322: Type 'IMemberService | MockMemberService' is not assignable to type 'IMemberService'.
6.367   Property 'updateMemberBiography' is missing in type 'MockMemberService' but required in type 'IMemberService'.
6.367 src/services/service.factory.ts(84,5): error TS2322: Type 'IRelationshipService | MockRelationshipService' is not assignable to type 'IRelationshipService'.
6.367   Property 'addItems' is missing in type 'MockRelationshipService' but required in type 'IRelationshipService'.
8.198 src/services/aiBiography/mock.aiBiography.service.ts(22,7): error TS2353: Object literal may only specify known properties, and 'provider' does not exist in type 'BiographyResultDto'.
8.198 src/services/member/mock.member.service.ts(345,14): error TS2420: Class 'MockMemberService' incorrectly implements interface 'IMemberService'.
8.198   Property 'updateMemberBiography' is missing in type 'MockMemberService' but required in type 'IMemberService'.
8.198 src/services/relationship/mock.relationship.service.ts(4,14): error TS2420: Class 'MockRelationshipService' incorrectly implements interface 'IRelationshipService'.
8.198   Property 'addItems' is missing in type 'MockRelationshipService' but required in type 'IRelationshipService'.
8.198 src/services/service.factory.ts(72,5): error TS2322: Type 'IMemberService | MockMemberService' is not assignable to type 'IMemberService'.
8.198   Property 'updateMemberBiography' is missing in type 'MockMemberService' but required in type 'IMemberService'.
8.198 src/services/service.factory.ts(84,5): error TS2322: Type 'IRelationshipService | MockRelationshipService' is not assignable to type 'IRelationshipService'.
8.198   Property 'addItems' is missing in type 'MockRelationshipService' but required in type 'IRelationshipService'.
8.198 tests/shared/mock.services.ts(39,14): error TS2420: Class 'MockMemberService' incorrectly implements interface 'IMemberService'.
8.198   Property 'updateMemberBiography' is missing in type 'MockMemberService' but required in type 'IMemberService'.
8.198 tests/unit/stores/notification.store.spec.ts(33,11): error TS2551: Property 'hideSnackbar' does not exist on type 'Store<"notification", { snackbar: SnackbarState; }, {}, { showSnackbar(message: string, color?: string, timeout?: number): void; resetSnackbar(): void; }>'. Did you mean 'snackbar'?
8.198 tests/unit/views/family/FamilyListView.spec.ts(122,7): error TS2420: Class 'MockMemberServiceForTest' incorrectly implements interface 'IMemberService'.
8.198   Property 'updateMemberBiography' is missing in type 'MockMemberServiceForTest' but required in type 'IMemberService'.
8.198 tests/unit/views/family/FamilyListView.spec.ts(372,51): error TS2741: Property 'updateMemberBiography' is missing in type 'MockMemberServiceForTest' but required in type 'IMemberService'.
8.198 tests/unit/views/members/MemberListView.spec.ts(23,7): error TS2420: Class 'MockMemberServiceForTest' incorrectly implements interface 'IMemberService'.
8.198   Property 'updateMemberBiography' is missing in type 'MockMemberServiceForTest' but required in type 'IMemberService'.
8.198 tests/unit/views/members/MemberListView.spec.ts(151,7): error TS2741: Property 'updateMemberBiography' is missing in type 'MockMemberServiceForTest' but required in type 'IMemberService'.
8.233 ERROR: "type-check" exited with 2.
------
Dockerfile.frontend:17

--------------------

  15 |     

  16 |     # Build the frontend application.

  17 | >>> RUN npm run build

  18 |     

  19 |     # Production stage

--------------------

target frontend: failed to solve: process "/bin/sh -c npm run build" did not complete successfully: exit code: 1


 *  The terminal process "/bin/zsh '-l', '-c', 'docker compose -f 'src/infra/docker-compose.yml' up -d --build'" terminated with exit code: 1. 
 *  Terminal will be reused by tasks, press any key to close it. 