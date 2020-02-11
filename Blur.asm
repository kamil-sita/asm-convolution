.CODE

;filterasm
; applies 1-dimensional convolutional filter
; corresponding header in high level programming language (with assumption that size of int is 4 bytes, and double is 8 bytes):
; filter(double* kernelPtr, int* imgInPtr, int* imgOutPtr, int kernelSize, int imgStartX, int imgStartY, int imgEndX, int imgEndY, int imgWidth, int imgJmp)
;
; Location of each argument after moving them back to registers (see line 34)
; ------------------------------------------------------------------
; rcx                     - kernelPtr  
; rdx                     - imgInPtr   
; r8                      - imgOutPtr  
; r9                      - kernelSize 
; qword ptr [rbp + 48]    - imgStartX  
; qword ptr [rbp + 56]    - imgStartY  
; qword ptr [rbp + 64]    - imgEndX    
; qword ptr [rbp + 72]    - imgEndY    
; r11                     - imgWidth   
; r12                     - imgJmp     
filterasm PROC

	;preserving registers
	push rbp 
	mov rbp, rsp 
	push r12 
	push r13 
	push r14 
	push r15 
	push rbx 

	;moving arguments from stack to registers
    mov r11, qword ptr [rbp + 80] ;moving imgWidth to register r11
    mov r12, qword ptr [rbp + 88] ;moving imgJmp to register r12
	;all variables moved to where we expect them

    ;algorithm
	shr r9, 1  ; kernelSizeHalf = kernelSize/2
	

		loop_y:
		mov r13, qword ptr [rbp + 56] ; 

		loop_y_cmp:
		cmp r13, qword ptr [rbp + 72]   
		jge loop_y_end					

			loop_x:
			mov r14, qword ptr [rbp + 48] 

			loop_x_cmp:
			cmp r14, qword ptr [rbp + 64] 
			jge loop_x_end				  

				pixel_loop:
				xorps xmm0, xmm0 

				; 2D -> 1D translation
				mov rbx, r13
				imul rbx, r11 
				add rbx, r14 ; pixel address

				mov r15, r12 
				imul r15, r9 
				mov rsi, rbx 
				sub rsi, r15 ; kernel address

					loop_k_init:
					mov r15, r9 
					neg r15

					loop_k_cmp:
					cmp r15, r9
					je last_iteration

						; at least 2 more iterations - 2 iterations done at once
						two_or_more_iterations: 
						mov r10, r15
						add r10, r9 

						;packing two adjacent kernel values
						movsd xmm1, qword ptr [rcx + 8 * r10]
						inc r10
						movsd xmm2, qword ptr [rcx + 8 * r10]
						movlhps xmm1, xmm2 

						;packing two adjacent pixel values (and converting them from byte to double)
						
						xor r10, r10

						; conversion from byte to double
						mov r10b, byte ptr[rdx + rsi]
						cvtsi2sd xmm2, r10

						add rsi, r12 

						; conversion from byte to double
						mov r10b, byte ptr[rdx + rsi] 
						cvtsi2sd xmm3, r10d ;
						
					
						movlhps xmm2, xmm3

						; multiplying those packed pixel and kernel values. Adding them to partial sums.
						mulpd xmm2, xmm1 
						addpd xmm0, xmm2 
					

					loop_k_inc:
					add r15, 2
					add rsi, r12
					jmp loop_k_cmp
					
						; last iteration
						last_iteration:
						mov r10, r15
						add r10, r9 

						movsd xmm1, qword ptr [rcx + 8 * r10]

						; conversion from byte to double 
						xor r10, r10 
						mov r10b, byte ptr[rdx + rsi] 
						cvtsi2sd xmm2, r10

						mulsd xmm2, xmm1
						addsd xmm0, xmm2 

					loop_k_end:

				; summing partial sums 
				movhlps xmm3, xmm0 
				addsd xmm0, xmm3
				
				; converting from double to byte
				cvttsd2si rdi, xmm0

				; saving result to imgOutPtr
				mov byte ptr [r8 + rbx], dil

			loop_x_inc:
			inc r14
			jmp loop_x_cmp 

			loop_x_end:

		loop_y_inc: 
		inc r13
		jmp loop_y_cmp

		loop_y_end:


	; recreating registers
	pop rbx 
	pop r15 
	pop r14 
	pop r13 
	pop r12 
	pop rbp 
	ret

filterasm ENDP 

end